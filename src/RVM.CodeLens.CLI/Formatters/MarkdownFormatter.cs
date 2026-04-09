using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.CLI.Formatters;

public class MarkdownFormatter : IOutputFormatter
{
    public void FormatSolution(SolutionAnalysis analysis, TextWriter writer)
    {
        writer.WriteLine($"# {analysis.SolutionName}");
        writer.WriteLine();
        writer.WriteLine($"**Projects:** {analysis.Projects.Count} | **Analyzed:** {analysis.AnalyzedAt:yyyy-MM-dd HH:mm} UTC");
        writer.WriteLine();
        writer.WriteLine("| Project | Framework | Files | LOC | Classes | Methods | Avg CC | Avg MI |");
        writer.WriteLine("|---------|-----------|------:|----:|--------:|--------:|-------:|-------:|");

        foreach (var p in analysis.Projects)
        {
            writer.WriteLine($"| {p.Name} | {p.TargetFramework} | {p.Metrics.FileCount} | {p.Metrics.CodeLines:N0} | {p.Metrics.ClassCount} | {p.Metrics.MethodCount} | {p.Metrics.AverageCyclomaticComplexity:F1} | {p.Metrics.AverageMaintainabilityIndex:F1} |");
        }

        var totalLoc = analysis.Projects.Sum(p => p.Metrics.CodeLines);
        writer.WriteLine($"| **Total** | | {analysis.Projects.Sum(p => p.Metrics.FileCount)} | **{totalLoc:N0}** | | | | |");
        writer.WriteLine();

        FormatArchitecture(analysis.Architecture, writer);
    }

    public void FormatMetrics(SolutionAnalysis analysis, TextWriter writer)
    {
        writer.WriteLine("# Code Metrics");
        writer.WriteLine();

        foreach (var project in analysis.Projects)
        {
            writer.WriteLine($"## {project.Name}");
            writer.WriteLine();
            writer.WriteLine("| File | Lines | Code | Types |");
            writer.WriteLine("|------|------:|-----:|------:|");

            foreach (var file in project.Metrics.Files)
            {
                var fileName = Path.GetFileName(file.FilePath);
                writer.WriteLine($"| {fileName} | {file.TotalLines} | {file.CodeLines} | {file.Types.Count} |");
            }

            writer.WriteLine();
        }
    }

    public void FormatDependencies(DependencyGraph graph, TextWriter writer)
    {
        writer.WriteLine("# Dependency Graph");
        writer.WriteLine();

        var projectNodes = graph.Nodes.Where(n => n.Type == "project").ToList();

        foreach (var node in projectNodes)
        {
            writer.WriteLine($"## {node.Label}");

            var refs = graph.Edges
                .Where(e => e.Source == node.Id && e.Type == "reference")
                .Select(e => graph.Nodes.First(n => n.Id == e.Target).Label)
                .ToList();

            if (refs.Count > 0)
            {
                writer.WriteLine("- **Project References:** " + string.Join(", ", refs));
            }

            var pkgs = graph.Edges
                .Where(e => e.Source == node.Id && e.Type == "package")
                .Select(e => graph.Nodes.First(n => n.Id == e.Target).Label)
                .ToList();

            if (pkgs.Count > 0)
            {
                writer.WriteLine("- **NuGet Packages:** " + string.Join(", ", pkgs));
            }

            writer.WriteLine();
        }
    }

    public void FormatHotSpots(List<HotSpot> hotSpots, TextWriter writer)
    {
        writer.WriteLine("# Hot Spots");
        writer.WriteLine();

        if (hotSpots.Count == 0)
        {
            writer.WriteLine("No hot spots found.");
            return;
        }

        writer.WriteLine("| File | Commits | Authors | Avg CC | Score |");
        writer.WriteLine("|------|--------:|--------:|-------:|------:|");

        foreach (var spot in hotSpots.Take(20))
        {
            writer.WriteLine($"| {spot.FilePath} | {spot.CommitCount} | {spot.AuthorCount} | {spot.Complexity:F1} | {spot.Score:F1} |");
        }

        writer.WriteLine();
    }

    public void FormatArchitecture(ArchitectureAnalysis architecture, TextWriter writer)
    {
        writer.WriteLine("## Architecture");
        writer.WriteLine();
        writer.WriteLine("| Layer | Projects |");
        writer.WriteLine("|-------|----------|");

        foreach (var layer in architecture.Layers)
        {
            writer.WriteLine($"| {layer.Name} | {string.Join(", ", layer.Projects)} |");
        }

        if (architecture.Violations.Count > 0)
        {
            writer.WriteLine();
            writer.WriteLine("### Violations");
            foreach (var v in architecture.Violations)
            {
                writer.WriteLine($"- {v}");
            }
        }

        writer.WriteLine();
    }
}
