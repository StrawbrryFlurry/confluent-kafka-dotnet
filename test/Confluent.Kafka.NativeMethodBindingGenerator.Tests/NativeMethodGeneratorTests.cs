using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Confluent.Kafka.NativeMethodBindingGenerator.Tests;

public sealed class NativeMethodGeneratorTests
{
    private const string TestInputClass = """
      using System;
      using System.Runtime.InteropServices;
      using DynamicNativeMethodBinding;

      namespace TestNamespace;

      [NativeBindingsFrom(Name = "LibraryOne", Dll = "library-one")]
      [NativeBindingsFrom(Name = "LibraryTwo", Dll = "library-two")]
      internal static unsafe partial class DynamicNativeMethods {
          public static extern IntPtr ExternMethodWithoutAttribute();
      
          [GenerateAccessorMethod(MethodName = "DynamicExternMethod")]
          [DynamicDllImport]
          public static extern void dynamic_extern_method();
          
          [GenerateAccessorMethod(MethodName = "DynamicExternMethodWithCallingConvention")]
          [DynamicDllImport(CallingConvention = CallingConvention.Cdecl)]
          public static extern IntPtr dynamic_extern_method_with_calling_convention();
          
          [GenerateAccessorMethod(MethodName = "DynamicExternMethodWithArguments")]
          [DynamicDllImport]
          public static extern IntPtr dynamic_extern_method_with_arguments(string arg1, int arg2);
          
          [DynamicDllImport]
          public static extern IntPtr dynamic_extern_method_no_accessor();
      }
      """;

    [Fact]
    public void DynamicMethodBindingAttributes()
    {
        var result = RunGenerator();

        var generatedAttributes = result.GeneratedTrees.Single(x => x.FilePath.EndsWith("DynamicNativeMethodBindingAttributes.g.cs"));

        var generatedText = generatedAttributes.GetText().ToString();
        Assert.Contains("namespace DynamicNativeMethodBinding;", generatedText);
        Assert.Contains("internal sealed class NativeBindingsFromAttribute", generatedText);
        Assert.Contains("internal sealed class DynamicDllImportAttribute", generatedText);
        Assert.Contains("internal sealed class GenerateAccessorMethodAttribute", generatedText);
    }

    [Fact]
    public void DynamicMethodBingingInterfacePerTarget()
    {
        var result = RunGenerator();

        var bindingsInterface = result.GeneratedTrees.Single(x => x.FilePath.EndsWith("IDynamicNativeMethodsBindings.g.cs"));

        AssertSourceEquals(
            """
            namespace TestNamespace;
            partial class DynamicNativeMethods
            {
                public interface IDynamicNativeMethodsBindings
                {
                }
            }
            """,
            bindingsInterface.GetText().ToString());
    }

    [Fact]
    public void PartialBinderClassWithDelegateBackingFieldsAndAccessors()
    {
        var result = RunGenerator();

        var binderClass = result.GeneratedTrees.Single(x => x.FilePath.EndsWith("DynamicNativeMethods.Binder.g.cs"));

        Assert.Contains(
            """
            namespace TestNamespace;
            unsafe partial class DynamicNativeMethods
            {
                private static delegate*<void> _dynamic_extern_method;
                [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public static void DynamicExternMethod() => _dynamic_extern_method();
                private static delegate*<global::System.IntPtr> _dynamic_extern_method_with_calling_convention;
                [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public static global::System.IntPtr DynamicExternMethodWithCallingConvention() => _dynamic_extern_method_with_calling_convention();
                private static delegate*<string, int, global::System.IntPtr> _dynamic_extern_method_with_arguments;
                [global::System.Runtime.CompilerServices.MethodImplAttribute(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public static global::System.IntPtr DynamicExternMethodWithArguments(string arg1, int arg2) => _dynamic_extern_method_with_arguments(arg1, arg2);
                private static delegate*<global::System.IntPtr> _dynamic_extern_method_no_accessor;
            """,
            binderClass.GetText().ToString());
    }

    [Fact]
    public void BinderDynamicBindingMethodForAllBindingProviders()
    {
        var result = RunGenerator();

        var binderClass = result.GeneratedTrees.Single(x => x.FilePath.EndsWith("DynamicNativeMethods.Binder.g.cs"));

        Assert.Contains(
            """
                    internal static void BindDynamicNativeMethodsFrom<TBindingsProvider>()
                        where TBindingsProvider : IDynamicNativeMethodsBindings
                    {
                        if (typeof(TBindingsProvider) == typeof(LibraryOneBindings))
                        {
                            LibraryOneBindings.BindIntoDynamicNativeMethods();
                            return;
                        }
                
                        if (typeof(TBindingsProvider) == typeof(LibraryTwoBindings))
                        {
                            LibraryTwoBindings.BindIntoDynamicNativeMethods();
                            return;
                        }
                
                        throw new System.ArgumentException("Unknown binding type", nameof(TBindingsProvider));
                    }
                """.ReplaceLineEndings(""),
            binderClass.GetText().ToString().ReplaceLineEndings(""));
    }

    [Fact]
    public void BindingProviderWithLibraryNameAndBindMethodForTarget()
    {
        var result = RunGenerator();

        var providerLibraryOne = result.GeneratedTrees.Single(x => x.FilePath.EndsWith("DynamicNativeMethods.LibraryOne.Bindings.g.cs"));
        var providerLibraryTwo = result.GeneratedTrees.Single(x => x.FilePath.EndsWith("DynamicNativeMethods.LibraryTwo.Bindings.g.cs"));

        AssertSourceEquals(
            """
            namespace TestNamespace;
            unsafe partial class DynamicNativeMethods
            {
                internal sealed unsafe class LibraryOneBindings : IDynamicNativeMethodsBindings
                {
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-one")]
                    public static extern void dynamic_extern_method();
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-one", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.Cdecl)]
                    public static extern global::System.IntPtr dynamic_extern_method_with_calling_convention();
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-one")]
                    public static extern global::System.IntPtr dynamic_extern_method_with_arguments(string arg1, int arg2);
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-one")]
                    public static extern global::System.IntPtr dynamic_extern_method_no_accessor();
                    internal static unsafe void BindIntoDynamicNativeMethods()
                    {
                        DynamicNativeMethods._dynamic_extern_method = (delegate*<void> )&LibraryOneBindings.dynamic_extern_method;
                        DynamicNativeMethods._dynamic_extern_method_with_calling_convention = (delegate*<global::System.IntPtr> )&LibraryOneBindings.dynamic_extern_method_with_calling_convention;
                        DynamicNativeMethods._dynamic_extern_method_with_arguments = (delegate*<string, int, global::System.IntPtr> )&LibraryOneBindings.dynamic_extern_method_with_arguments;
                        DynamicNativeMethods._dynamic_extern_method_no_accessor = (delegate*<global::System.IntPtr> )&LibraryOneBindings.dynamic_extern_method_no_accessor;
                    }
                }
            }
            """,
            providerLibraryOne.GetText().ToString());

        AssertSourceEquals(
            """
            namespace TestNamespace;
            unsafe partial class DynamicNativeMethods
            {
                internal sealed unsafe class LibraryTwoBindings : IDynamicNativeMethodsBindings
                {
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-two")]
                    public static extern void dynamic_extern_method();
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-two", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.Cdecl)]
                    public static extern global::System.IntPtr dynamic_extern_method_with_calling_convention();
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-two")]
                    public static extern global::System.IntPtr dynamic_extern_method_with_arguments(string arg1, int arg2);
                    [global::System.Runtime.InteropServices.DllImportAttribute("library-two")]
                    public static extern global::System.IntPtr dynamic_extern_method_no_accessor();
                    internal static unsafe void BindIntoDynamicNativeMethods()
                    {
                        DynamicNativeMethods._dynamic_extern_method = (delegate*<void> )&LibraryTwoBindings.dynamic_extern_method;
                        DynamicNativeMethods._dynamic_extern_method_with_calling_convention = (delegate*<global::System.IntPtr> )&LibraryTwoBindings.dynamic_extern_method_with_calling_convention;
                        DynamicNativeMethods._dynamic_extern_method_with_arguments = (delegate*<string, int, global::System.IntPtr> )&LibraryTwoBindings.dynamic_extern_method_with_arguments;
                        DynamicNativeMethods._dynamic_extern_method_no_accessor = (delegate*<global::System.IntPtr> )&LibraryTwoBindings.dynamic_extern_method_no_accessor;
                    }
                }
            }
            """,
            providerLibraryTwo.GetText().ToString());
    }

    private static void AssertSourceEquals(string expected, string actual)
    {
        Assert.Equal(
            expected,
            actual,
            ignoreLineEndingDifferences: true);
    }

    private static GeneratorDriverRunResult RunGenerator()
    {
        var generator = new NativeMethodGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(NativeMethodGeneratorTests),
            [CSharpSyntaxTree.ParseText(TestInputClass)],
            [
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ]);

        var results = driver.RunGenerators(compilation).GetRunResult();
        Assert.True(results.Diagnostics.IsEmpty, $"Generator run reported diagnostics. {string.Join(Environment.NewLine, results.Diagnostics)}");
        return results;
    }
}