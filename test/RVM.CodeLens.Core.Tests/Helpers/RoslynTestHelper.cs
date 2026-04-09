using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RVM.CodeLens.Core.Tests.Helpers;

public static class RoslynTestHelper
{
    public static SyntaxTree Parse(string code) =>
        CSharpSyntaxTree.ParseText(code);

    public static (SemanticModel Model, CSharpCompilation Compilation) GetSemanticModel(string code)
    {
        var tree = Parse(code);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
        };

        // Add runtime references
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeRef = MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll"));

        var compilation = CSharpCompilation.Create("TestAssembly",
            [tree],
            [..references, runtimeRef],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return (compilation.GetSemanticModel(tree), compilation);
    }
}
