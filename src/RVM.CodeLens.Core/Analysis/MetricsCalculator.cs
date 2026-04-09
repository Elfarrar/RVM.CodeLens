using RVM.CodeLens.Core.Models;
using RVM.CodeLens.Core.Roslyn;
using RVM.CodeLens.Core.Roslyn.SyntaxWalkers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace RVM.CodeLens.Core.Analysis;

public class MetricsCalculator : IMetricsCalculator
{
    private readonly ILogger<MetricsCalculator> _logger;

    public MetricsCalculator(ILogger<MetricsCalculator> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectMetrics> CalculateAsync(Project project, CancellationToken ct = default)
    {
        var compilation = await project.GetCompilationAsync(ct);
        if (compilation is null)
        {
            _logger.LogWarning("Could not get compilation for {Project}", project.Name);
            return EmptyMetrics();
        }

        var fileMetricsList = new List<FileMetrics>();
        var totalLines = 0;
        var codeLines = 0;
        var blankLines = 0;
        var commentLines = 0;
        var classCount = 0;
        var methodCount = 0;
        var allComplexities = new List<int>();
        var allMaintainabilities = new List<double>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            ct.ThrowIfCancellationRequested();

            var filePath = tree.FilePath;
            if (string.IsNullOrEmpty(filePath) || IsGeneratedFile(filePath))
                continue;

            var lineCounts = LineCountWalker.Count(tree);
            totalLines += lineCounts.TotalLines;
            codeLines += lineCounts.CodeLines;
            blankLines += lineCounts.BlankLines;
            commentLines += lineCounts.CommentLines;

            var semanticModel = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync(ct);

            var typeMetricsList = new List<TypeMetrics>();
            var typeDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            foreach (var typeDecl in typeDeclarations)
            {
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (typeSymbol is null) continue;

                classCount++;
                var kind = typeDecl switch
                {
                    ClassDeclarationSyntax => "class",
                    RecordDeclarationSyntax r => r.ClassOrStructKeyword.Text == "struct" ? "record struct" : "record",
                    StructDeclarationSyntax => "struct",
                    InterfaceDeclarationSyntax => "interface",
                    _ => "type"
                };

                var methods = typeDecl.Members.OfType<MethodDeclarationSyntax>().ToList();
                var constructors = typeDecl.Members.OfType<ConstructorDeclarationSyntax>().ToList();
                var allMethods = new List<BaseMethodDeclarationSyntax>();
                allMethods.AddRange(methods);
                allMethods.AddRange(constructors);

                var methodMetricsList = new List<MethodMetrics>();

                foreach (var method in allMethods)
                {
                    methodCount++;
                    var methodName = method switch
                    {
                        MethodDeclarationSyntax m => m.Identifier.Text,
                        ConstructorDeclarationSyntax c => c.Identifier.Text + ".ctor",
                        _ => "unknown"
                    };

                    // Cyclomatic complexity
                    var complexityWalker = new ComplexityWalker();
                    complexityWalker.Visit(method);
                    var cc = complexityWalker.Complexity;
                    allComplexities.Add(cc);

                    // Line count for method
                    var methodSpan = method.Span;
                    var methodStartLine = tree.GetLineSpan(methodSpan).StartLinePosition.Line;
                    var methodEndLine = tree.GetLineSpan(methodSpan).EndLinePosition.Line;
                    var methodLines = methodEndLine - methodStartLine + 1;

                    // Class coupling
                    var couplingWalker = new CouplingWalker(semanticModel, typeSymbol.Name);
                    couplingWalker.Visit(method);
                    var coupling = couplingWalker.CouplingCount;

                    // Depth of inheritance
                    var doi = DepthOfInheritanceCalculator.Calculate(typeSymbol);

                    // Maintainability index (simplified)
                    var mi = MaintainabilityIndexCalculator.CalculateSimplified(methodLines, cc);
                    allMaintainabilities.Add(mi);

                    methodMetricsList.Add(new MethodMetrics(
                        methodName, methodLines, cc, coupling, doi));
                }

                var typeLines = typeDecl.Span.Length > 0
                    ? tree.GetLineSpan(typeDecl.Span).EndLinePosition.Line -
                      tree.GetLineSpan(typeDecl.Span).StartLinePosition.Line + 1
                    : 0;

                var typeMi = methodMetricsList.Count > 0
                    ? methodMetricsList.Average(m =>
                        MaintainabilityIndexCalculator.CalculateSimplified(m.LineCount, m.CyclomaticComplexity))
                    : 100;

                typeMetricsList.Add(new TypeMetrics(
                    typeSymbol.Name, kind, typeLines, methodMetricsList.Count,
                    Math.Round(typeMi, 2), methodMetricsList));
            }

            fileMetricsList.Add(new FileMetrics(
                filePath, lineCounts.TotalLines, lineCounts.CodeLines, typeMetricsList));
        }

        var avgComplexity = allComplexities.Count > 0 ? allComplexities.Average() : 0;
        var avgMaintainability = allMaintainabilities.Count > 0 ? allMaintainabilities.Average() : 100;

        return new ProjectMetrics(
            fileMetricsList.Count, totalLines, codeLines, blankLines, commentLines,
            classCount, methodCount,
            Math.Round(avgComplexity, 2), Math.Round(avgMaintainability, 2),
            fileMetricsList);
    }

    private static bool IsGeneratedFile(string filePath)
    {
        var name = Path.GetFileName(filePath);
        return name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
               filePath.Contains("obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
               filePath.Contains("obj" + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static ProjectMetrics EmptyMetrics() =>
        new(0, 0, 0, 0, 0, 0, 0, 0, 100, []);
}
