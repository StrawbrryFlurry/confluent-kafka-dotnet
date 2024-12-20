using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Confluent.Kafka.NativeMethodBindingGenerator;

[Generator]
public class NativeMethodGenerator : IIncrementalGenerator
{
    private const string DynamicNativeMethodBindingAttributeSourceCode = """
        namespace DynamicNativeMethodBinding;

        [System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        internal sealed class NativeBindingsFromAttribute : global::System.Attribute {
            public string Name { get; set; }
            public string Dll { get; set; }
        }
         
        [System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        internal sealed class DynamicDllImportAttribute : global::System.Attribute {
            public global::System.Runtime.InteropServices.CallingConvention CallingConvention { get; set; }
        }
         
        [System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
            internal sealed class GenerateAccessorMethodAttribute : global::System.Attribute {
            public string MethodName { get; set; }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "DynamicNativeMethodBindingAttributes.g.cs",
            SourceText.From(DynamicNativeMethodBindingAttributeSourceCode, Encoding.UTF8)));

        var classesWithNativeBindings = context.SyntaxProvider.ForAttributeWithMetadataName(
            "DynamicNativeMethodBinding.NativeBindingsFromAttribute",
            (node, _) =>
            {
                if (node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } cls)
                {
                    return false;
                }
                
                return cls.AttributeLists.SelectMany(x => x.Attributes)
                    .Any(x => x.Name.ToString().Contains("NativeBindingsFrom")); // Doesn't need to be 100% accurate, just filter out the obvious non-matches
            },
            (ctx, _) =>
            {
                var classNode = (ClassDeclarationSyntax)ctx.TargetNode;
                // If there was a false positive before, this simply returns an empty array
                var bindings = GetNativeMethodBindingsForClass(classNode, ctx.SemanticModel);
                return new ClassWithNativeBindings(classNode, bindings);
            });

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(classesWithNativeBindings.Collect()),
            (ctx, t) => GenerateNativeBindings(ctx, t.Left, t.Right));
    }

    private static (string BindingName, string Dll)[] GetNativeMethodBindingsForClass(
        ClassDeclarationSyntax cls,
        SemanticModel sm)
    {
        var classSymbol = sm.GetDeclaredSymbol(cls)!;
        var nativeBindingsAttributeSymbol = sm.Compilation.GetTypeByMetadataName("DynamicNativeMethodBinding.NativeBindingsFromAttribute")!;
        return classSymbol.GetAttributes()
            .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, nativeBindingsAttributeSymbol))
            .Select(a =>
            {
                var bindingName = a.NamedArguments.FirstOrDefault(x => x.Key == "Name");
                var targetDll = a.NamedArguments.FirstOrDefault(x => x.Key == "Dll");
                return ((string)bindingName.Value.Value!, (string)targetDll.Value.Value!);
            }).ToArray();
    }
    
    private record struct ClassWithNativeBindings(
        ClassDeclarationSyntax ClassDeclaration,
        (string BindingName, string Dll)[] Bindings);
    
    private void GenerateNativeBindings(
        SourceProductionContext ctx,
        Compilation compilation,
        ImmutableArray<ClassWithNativeBindings> classesWithBindings)
    { 
        foreach (var bindings in classesWithBindings)
        {
            try
            {
                NativeMethodGeneratorImpl.GenerateNativeBindingsForClass(
                    ctx,
                    compilation,
                    bindings.ClassDeclaration,
                    bindings.Bindings);
            }
            catch (Exception e)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "NMG001",
                        "Error generating native method bindings",
                        "Error generating native method bindings: {0}\n{1}",
                        "NativeMethodGenerator",
                        DiagnosticSeverity.Error,
                        true
                    ),
                    Location.None,
                    e.Message,
                    e.StackTrace));
            }
        }
    }
}