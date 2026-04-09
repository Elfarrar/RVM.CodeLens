using RVM.CodeLens.Core.Models;
using Spectre.Console;

namespace RVM.CodeLens.CLI.Formatters;

public class TableFormatter : IOutputFormatter
{
    public void FormatSolution(SolutionAnalysis analysis, TextWriter writer)
    {
        AnsiConsole.MarkupLine($"[bold blue]Solution:[/] {analysis.SolutionName}");
        AnsiConsole.MarkupLine($"[bold]Projects:[/] {analysis.Projects.Count}");
        AnsiConsole.MarkupLine($"[bold]Analyzed at:[/] {analysis.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Project[/]")
            .AddColumn("[bold]Framework[/]")
            .AddColumn(new TableColumn("[bold]Files[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]LOC[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Classes[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Methods[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Avg CC[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Avg MI[/]").RightAligned());

        foreach (var project in analysis.Projects)
        {
            var cc = project.Metrics.AverageCyclomaticComplexity;
            var mi = project.Metrics.AverageMaintainabilityIndex;

            table.AddRow(
                project.Name,
                project.TargetFramework,
                project.Metrics.FileCount.ToString(),
                project.Metrics.CodeLines.ToString("N0"),
                project.Metrics.ClassCount.ToString(),
                project.Metrics.MethodCount.ToString(),
                ColorizeComplexity(cc),
                ColorizeMaintainability(mi));
        }

        var totalLoc = analysis.Projects.Sum(p => p.Metrics.CodeLines);
        var totalFiles = analysis.Projects.Sum(p => p.Metrics.FileCount);
        table.AddEmptyRow();
        table.AddRow("[bold]TOTAL[/]", "", totalFiles.ToString(), totalLoc.ToString("N0"), "", "", "", "");

        AnsiConsole.Write(table);

        FormatArchitecture(analysis.Architecture, writer);
    }

    public void FormatMetrics(SolutionAnalysis analysis, TextWriter writer)
    {
        foreach (var project in analysis.Projects)
        {
            AnsiConsole.MarkupLine($"\n[bold blue]{project.Name}[/]");

            var table = new Table()
                .Border(TableBorder.Simple)
                .AddColumn("[bold]File[/]")
                .AddColumn(new TableColumn("[bold]Lines[/]").RightAligned())
                .AddColumn(new TableColumn("[bold]Code[/]").RightAligned())
                .AddColumn(new TableColumn("[bold]Types[/]").RightAligned());

            foreach (var file in project.Metrics.Files)
            {
                var fileName = Path.GetFileName(file.FilePath);
                table.AddRow(fileName, file.TotalLines.ToString(), file.CodeLines.ToString(),
                    file.Types.Count.ToString());
            }

            AnsiConsole.Write(table);

            // Show method-level details for complex methods
            var complexMethods = project.Metrics.Files
                .SelectMany(f => f.Types)
                .SelectMany(t => t.Methods.Select(m => new { Type = t.Name, Method = m }))
                .Where(x => x.Method.CyclomaticComplexity > 5)
                .OrderByDescending(x => x.Method.CyclomaticComplexity)
                .Take(10);

            if (complexMethods.Any())
            {
                AnsiConsole.MarkupLine($"\n  [yellow]Complex methods (CC > 5):[/]");
                var methodTable = new Table()
                    .Border(TableBorder.Simple)
                    .AddColumn("Type")
                    .AddColumn("Method")
                    .AddColumn(new TableColumn("CC").RightAligned())
                    .AddColumn(new TableColumn("Lines").RightAligned())
                    .AddColumn(new TableColumn("Coupling").RightAligned());

                foreach (var m in complexMethods)
                {
                    methodTable.AddRow(
                        m.Type, m.Method.Name,
                        ColorizeComplexity(m.Method.CyclomaticComplexity),
                        m.Method.LineCount.ToString(),
                        m.Method.ClassCoupling.ToString());
                }

                AnsiConsole.Write(methodTable);
            }
        }
    }

    public void FormatDependencies(DependencyGraph graph, TextWriter writer)
    {
        AnsiConsole.MarkupLine("[bold blue]Dependency Graph[/]");

        var projectNodes = graph.Nodes.Where(n => n.Type == "project").ToList();
        var tree = new Tree("[bold]Projects[/]");

        foreach (var node in projectNodes)
        {
            var projectNode = tree.AddNode($"[blue]{node.Label}[/]");

            var projectRefs = graph.Edges
                .Where(e => e.Source == node.Id && e.Type == "reference")
                .ToList();

            if (projectRefs.Count > 0)
            {
                var refsNode = projectNode.AddNode("[dim]Project References[/]");
                foreach (var edge in projectRefs)
                {
                    var target = graph.Nodes.First(n => n.Id == edge.Target);
                    refsNode.AddNode($"→ {target.Label}");
                }
            }

            var packageRefs = graph.Edges
                .Where(e => e.Source == node.Id && e.Type == "package")
                .ToList();

            if (packageRefs.Count > 0)
            {
                var pkgNode = projectNode.AddNode("[dim]NuGet Packages[/]");
                foreach (var edge in packageRefs)
                {
                    var target = graph.Nodes.First(n => n.Id == edge.Target);
                    pkgNode.AddNode($"[green]{target.Label}[/]");
                }
            }
        }

        AnsiConsole.Write(tree);
    }

    public void FormatHotSpots(List<HotSpot> hotSpots, TextWriter writer)
    {
        AnsiConsole.MarkupLine("[bold blue]Hot Spots[/] (high churn × high complexity)");
        AnsiConsole.WriteLine();

        if (hotSpots.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No hot spots found (no git history or no .cs files changed)[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]File[/]")
            .AddColumn(new TableColumn("[bold]Commits[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Authors[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Avg CC[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Score[/]").RightAligned());

        foreach (var spot in hotSpots.Take(20))
        {
            var scoreColor = spot.Score switch
            {
                > 100 => "red",
                > 50 => "yellow",
                _ => "green"
            };

            table.AddRow(
                spot.FilePath,
                spot.CommitCount.ToString(),
                spot.AuthorCount.ToString(),
                ColorizeComplexity(spot.Complexity),
                $"[{scoreColor}]{spot.Score:F1}[/]");
        }

        AnsiConsole.Write(table);
    }

    public void FormatArchitecture(ArchitectureAnalysis architecture, TextWriter writer)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Architecture[/]");

        var table = new Table()
            .Border(TableBorder.Simple)
            .AddColumn("[bold]Layer[/]")
            .AddColumn("[bold]Projects[/]");

        foreach (var layer in architecture.Layers)
        {
            table.AddRow($"[bold]{layer.Name}[/]", string.Join(", ", layer.Projects));
        }

        AnsiConsole.Write(table);

        if (architecture.Violations.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold red]Architecture Violations:[/]");
            foreach (var violation in architecture.Violations)
            {
                AnsiConsole.MarkupLine($"  [red]✗[/] {violation}");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[green]No architecture violations detected.[/]");
        }
    }

    private static string ColorizeComplexity(double cc) => cc switch
    {
        <= 5 => $"[green]{cc:F1}[/]",
        <= 10 => $"[yellow]{cc:F1}[/]",
        <= 20 => $"[darkorange]{cc:F1}[/]",
        _ => $"[red]{cc:F1}[/]"
    };

    private static string ColorizeMaintainability(double mi) => mi switch
    {
        >= 80 => $"[green]{mi:F1}[/]",
        >= 50 => $"[yellow]{mi:F1}[/]",
        _ => $"[red]{mi:F1}[/]"
    };
}
