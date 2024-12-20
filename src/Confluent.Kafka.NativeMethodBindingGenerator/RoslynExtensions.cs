using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Confluent.Kafka.NativeMethodBindingGenerator;

internal static class RoslynExtensions
{
    internal static SyntaxToken Public => Token(SyntaxKind.PublicKeyword);
    internal static SyntaxToken Private => Token(SyntaxKind.PrivateKeyword);
    internal static SyntaxToken Static => Token(SyntaxKind.StaticKeyword);
    internal static SyntaxToken Unsafe => Token(SyntaxKind.UnsafeKeyword);
    internal static SyntaxToken Extern => Token(SyntaxKind.ExternKeyword);
    internal static SyntaxToken Internal => Token(SyntaxKind.InternalKeyword);
    internal static SyntaxToken Partial => Token(SyntaxKind.PartialKeyword);
    internal static SyntaxToken Sealed => Token(SyntaxKind.SealedKeyword);
    
    public static ArgumentListSyntax ToArgumentList(this ParameterListSyntax parameterList)
    {
        return ArgumentList(
            SeparatedList(parameterList.Parameters.Select(ArgumentFromParameter)));
    }
    
    private static ArgumentSyntax ArgumentFromParameter(ParameterSyntax parameter)
    {
        var argument = Argument(IdentifierName(parameter.Identifier));
        if (parameter.Modifiers.Count == 0)
        {
            return argument;
        }
        
        var refKind = parameter.Modifiers.Any(SyntaxKind.RefKeyword)
            ? SyntaxKind.RefKeyword
            : parameter.Modifiers.Any(SyntaxKind.OutKeyword)
                ? SyntaxKind.OutKeyword
                : parameter.Modifiers.Any(SyntaxKind.InKeyword)
                    ? SyntaxKind.InKeyword
                    : SyntaxKind.None;
        
        return argument.WithRefKindKeyword(Token(refKind));
    }
    
    public static ParameterListSyntax WithoutTrivia(this ParameterListSyntax parameterList)
    {
        return parameterList.WithParameters(SeparatedList(
            parameterList.Parameters.Select(x => x.WithoutTrivia())));
    }
    
    public static string ToStringifiedCompilationUnitInNamespace(this MemberDeclarationSyntax syntaxNode, string ns)
    {
        return CompilationUnit()
            .WithMembers(List<MemberDeclarationSyntax>([
                FileScopedNamespaceDeclaration(ParseName(ns)),
                syntaxNode]))
            .NormalizeWhitespace()
            .ToFullString();
    }
    
    public static MethodDeclarationSyntax WithTrailingSemicolon(this MethodDeclarationSyntax method)
    {
        return method.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
    
    public static MethodDeclarationSyntax WithAttribute(
        this MethodDeclarationSyntax method,
        AttributeSyntax attribute)
    {
         return method.WithAttributeLists(
            List([AttributeList(SeparatedList([attribute]))])
        );
    }
    
    public static bool TryGetAttribute(
        this ISymbol symbol,
        INamedTypeSymbol attributeType,
        out AttributeData? attribute)
    {
        attribute = symbol.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
        return attribute is not null;
    }
}