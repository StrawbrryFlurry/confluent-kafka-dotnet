using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Confluent.Kafka.NativeMethodBindingGenerator.RoslynExtensions;

namespace Confluent.Kafka.NativeMethodBindingGenerator;

internal sealed class NativeMethodGeneratorImpl
{
    private SourceProductionContext SourceCtx { get; }
    private INamedTypeSymbol ClassSymbol { get; }

    private SemanticModel SemanticModel { get; }

    private INamedTypeSymbol GenerateAccessorMethodAttribute { get; }
    private INamedTypeSymbol DynamicDllImportAttributeSymbol { get; }
    
    private string ClassName => ClassSymbol.Name;
    private string ClassNamespace => ClassSymbol.ContainingNamespace.ToDisplayString();
    
    private NativeMethodGeneratorImpl(
        SourceProductionContext sourceCtx,
        Compilation compilation,
        ClassDeclarationSyntax targetClass)
    {
        SourceCtx = sourceCtx;
        SemanticModel = compilation.GetSemanticModel(targetClass.SyntaxTree);
        ClassSymbol = SemanticModel.GetDeclaredSymbol(targetClass) 
                       ?? throw new InvalidOperationException("Target class symbol not found in compilation");
        
        DynamicDllImportAttributeSymbol = compilation.GetTypeByMetadataName("DynamicNativeMethodBinding.DynamicDllImportAttribute")!;
        GenerateAccessorMethodAttribute = compilation.GetTypeByMetadataName("DynamicNativeMethodBinding.GenerateAccessorMethodAttribute")!;
    }
    
    public static void GenerateNativeBindingsForClass(
        SourceProductionContext ctx,
        Compilation compilation,
        ClassDeclarationSyntax targetClass,
        (string BindingName, string Dll)[] bindings)
    {
        var instance = new NativeMethodGeneratorImpl(ctx, compilation, targetClass);
        instance.GenerateNativeBindings(bindings);
    }

    private void GenerateNativeBindings((string BindingName, string Dll)[] bindings)
    {
        var nativeMethodsToBind = FindDynamicallyBoundNativeMethods();
        
        var globalRewriter = new TypeToGloballyQualifiedIdentifierRewriter(SemanticModel);
        var nativeMethodsWithGlobalIdentifiers = nativeMethodsToBind
            .Select(x => x with { Declaration = (MethodDeclarationSyntax)globalRewriter.Visit(x.Declaration) })
            .ToImmutableArray();
        
        var bindingInterface = GenerateBindingsInterface();
        var binder = MakeBinderClass(nativeMethodsWithGlobalIdentifiers);
        
        foreach (var (bindingName, dll) in bindings)
        {
            GenerateProviderForBinding(nativeMethodsWithGlobalIdentifiers, dll, bindingName, bindingInterface);
        }
        
        AddDynamicBindMethod(ref binder, bindings, bindingInterface);
        
        var binderCode = binder.ToStringifiedCompilationUnitInNamespace(ClassNamespace);
        SourceCtx.AddSource($"{ClassName}.Binder.g.cs", SourceText.From(binderCode, Encoding.UTF8));
    }
    
    private string GenerateBindingsInterface()
    {
        var bindingInterfaceName = $"I{ClassName}Bindings";
        var bindingInterface = InterfaceDeclaration(bindingInterfaceName)
            .WithModifiers(TokenList(Public));
        var containerClass = ClassDeclaration(ClassName)
            .WithModifiers(TokenList(Partial))
            .WithMembers(List<MemberDeclarationSyntax>([bindingInterface]));

        var code = containerClass.ToStringifiedCompilationUnitInNamespace(ClassNamespace);
        SourceCtx.AddSource($"I{ClassName}Bindings.g.cs", SourceText.From(code, Encoding.UTF8));
        return bindingInterfaceName;
    }

    private ClassDeclarationSyntax MakeBinderClass(ImmutableArray<NativeMethodRef> methodsToBind)
    {
        var binderClass = ClassDeclaration(ClassName).WithModifiers(TokenList(Unsafe, Partial));

        foreach (var method in methodsToBind)
        {
            AddNativeMethodToBinderClass(method, ref binderClass);
        }

        return binderClass;
    }
    
    private void AddNativeMethodToBinderClass(
        NativeMethodRef method,
        ref ClassDeclarationSyntax binderClass)
    {
        var backingFieldType = MakeFunctionPointerTypeFromMethod(method.Declaration);
        var backingFieldIdentifier = Identifier(MakeNativeMethodBackingFieldName(method.Symbol));

        binderClass = binderClass.AddMembers(
            FieldDeclaration(
                VariableDeclaration(backingFieldType, SeparatedList([VariableDeclarator(backingFieldIdentifier)])))
            .WithModifiers(TokenList(Private, Static)));

        if (!method.Symbol.TryGetAttribute(GenerateAccessorMethodAttribute, out var attribute))
        {
            return;
        }

        var accessorMethodName = (string)attribute!.NamedArguments.FirstOrDefault(x => x.Key == "MethodName").Value.Value!;
        var accessorMethod = MethodDeclaration(method.Declaration.ReturnType, accessorMethodName)
            .WithAttribute(
                Attribute(
                    ParseName("global::System.Runtime.CompilerServices.MethodImplAttribute"),
                    AttributeArgumentList(SeparatedList([
                        AttributeArgument(ParseName("global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"))]))))
            .WithModifiers(TokenList(Public, Static))
            .WithParameterList(method.Declaration.ParameterList)
            .WithExpressionBody(ArrowExpressionClause(
                InvocationExpression(
                    IdentifierName(backingFieldIdentifier),
                    method.Declaration.ParameterList.ToArgumentList())))
            .WithTrailingSemicolon();
            
        binderClass = binderClass.AddMembers(accessorMethod);
    }

    private static string MakeNativeMethodBackingFieldName(IMethodSymbol method)
    {
        return $"_{method.Name}";
    }
    
    private void GenerateProviderForBinding(
        ImmutableArray<NativeMethodRef> methodsToBind,
        string dll,
        string bindingName,
        string bindingInterface)
    {
        var providerClassName = MakeBindingsProviderClassName(bindingName);
        var providerClass = ClassDeclaration(providerClassName)
            .WithModifiers(TokenList(Internal, Sealed, Unsafe))
            .WithBaseList(BaseList(SeparatedList((BaseTypeSyntax[])[SimpleBaseType(ParseTypeName(bindingInterface))])));
        
        foreach (var methodRef in methodsToBind)
        {
            var declaration = methodRef.Declaration;
            var dynamicImportAttribute = declaration.AttributeLists
                .SelectMany(x => x.Attributes)
                .Single(x => x.Name.ToString().Contains("DynamicDllImport"));

            var providerSpecificNativeMethod = MethodDeclaration(
                declaration.ReturnType,
                declaration.Identifier)
            .WithAttribute(
                Attribute(ParseName("global::System.Runtime.InteropServices.DllImportAttribute"))
                    .WithArgumentList(AttributeArgumentList(SeparatedList((AttributeArgumentSyntax[])[
                        AttributeArgument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(dll)
                            )),
                        // Forward all the dynamic import attribute arguments
                        ..dynamicImportAttribute.ArgumentList?.Arguments ?? []
                    ]))))
            .WithModifiers(TokenList(Public, Static, Extern))
            .WithParameterList(declaration.ParameterList.WithoutTrivia())
            .WithTrailingSemicolon();
            
            providerClass = providerClass.AddMembers(providerSpecificNativeMethod);
        }
        
        AddProviderBindMethodForBinding(ref providerClass, methodsToBind);
        
        var parentClass = ClassDeclaration(ClassName)
            .WithModifiers(TokenList(Unsafe, Partial))
            .WithMembers(List<MemberDeclarationSyntax>([providerClass]));

        var code = parentClass.ToStringifiedCompilationUnitInNamespace(ClassNamespace);
        SourceCtx.AddSource($"{ClassName}.{bindingName}.Bindings.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static string MakeBindingsProviderClassName(string bindingName)
    {
        return $"{bindingName}Bindings";
    }
    
    private void AddProviderBindMethodForBinding(
        ref ClassDeclarationSyntax providerClass,
        ImmutableArray<NativeMethodRef> methodsToBind)
    {
        var bindMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(MakeProviderBindMethodName()))
            .WithModifiers(TokenList(Internal, Static, Unsafe));
        
        var statements = new List<StatementSyntax>(methodsToBind.Length);
        foreach (var method in methodsToBind)
        {
            var backingFieldIdentifier = MakeNativeMethodBackingFieldName(method.Symbol);
            var backingFieldType = MakeFunctionPointerTypeFromMethod(method.Declaration);
            // _backingField = (delegate* <x>) &FullyQualifiedBindTarget.MethodName;
            var bindStatement = ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(ClassName),
                        IdentifierName(backingFieldIdentifier)),
                    CastExpression(
                        backingFieldType,
                        PrefixUnaryExpression(
                            SyntaxKind.AddressOfExpression,
                            Token(SyntaxKind.AmpersandToken),
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(providerClass.Identifier),
                                IdentifierName(method.Declaration.Identifier))))));
            statements.Add(bindStatement);
        }

        providerClass = providerClass.AddMembers(bindMethod.WithBody(Block(List(statements))));
    }

    private string MakeProviderBindMethodName()
    {
        return $"BindInto{ClassName}";
    }
    
    private void AddDynamicBindMethod(
        ref ClassDeclarationSyntax binder,
        (string BindingName, string Dll)[] bindings,
        string bindingInterface)
    {
        var dynamicBindMethod = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier($"Bind{ClassName}From"))
            .WithModifiers(TokenList(Internal, Static))
            .WithTypeParameterList(TypeParameterList(SeparatedList([TypeParameter("TBindingsProvider")])))
            .AddConstraintClauses(TypeParameterConstraintClause("TBindingsProvider")
                .WithConstraints(SeparatedList((TypeParameterConstraintSyntax[])[
                    TypeConstraint(ParseTypeName(bindingInterface))])));
        
        var statements = new List<StatementSyntax>(bindings.Length);
        var typeParameterType = TypeOfExpression(ParseTypeName("TBindingsProvider"));
        foreach (var (bindingName, _) in bindings)
        {
            var targetBindingType = TypeOfExpression(ParseTypeName(MakeBindingsProviderClassName(bindingName)));
            var bindImplementationCall = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(MakeBindingsProviderClassName(bindingName)),
                    IdentifierName(MakeProviderBindMethodName())));
            
            var matchBindStatement = IfStatement(
                BinaryExpression(SyntaxKind.EqualsExpression, typeParameterType, targetBindingType),
                Block(List<StatementSyntax>([
                    ExpressionStatement(bindImplementationCall),
                    ReturnStatement()])));

            statements.Add(matchBindStatement);
        }

        statements.Add(ThrowStatement(ObjectCreationExpression(ParseTypeName("System.ArgumentException"))
            .WithArgumentList(ArgumentList(SeparatedList((ArgumentSyntax[])[
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Unknown binding type"))),
                Argument(IdentifierName("nameof(TBindingsProvider)"))])))));
        
        binder = binder.AddMembers(dynamicBindMethod.WithBody(Block(List(statements))));
    }
    
    private ImmutableArray<NativeMethodRef> FindDynamicallyBoundNativeMethods()
    {
        var methods = ClassSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsExtern && m.TryGetAttribute(DynamicDllImportAttributeSymbol, out _));
        
        return methods.Select(m => new NativeMethodRef(
            (MethodDeclarationSyntax)m.DeclaringSyntaxReferences[0].GetSyntax(),
            m
        )).ToImmutableArray();
    }

    private record struct NativeMethodRef(
        MethodDeclarationSyntax Declaration,
        IMethodSymbol Symbol);

    private static FunctionPointerTypeSyntax MakeFunctionPointerTypeFromMethod(MethodDeclarationSyntax method)
    {
        return FunctionPointerType()
            .WithParameterList(
                FunctionPointerParameterList(SeparatedList((FunctionPointerParameterSyntax[])[
                    ..method.ParameterList.WithoutTrivia().Parameters.Select(x => FunctionPointerParameter([], x.Modifiers, x.Type ?? throw new InvalidOperationException("Type is null"))),
                    FunctionPointerParameter(method.ReturnType.WithoutTrivia())])));
    }
}