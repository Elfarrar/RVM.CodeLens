using System.Xml.Linq;
using RVM.CodeLens.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace RVM.CodeLens.Core.Analysis;

public class ProjectAnalyzer : IProjectAnalyzer
{
    private readonly IMetricsCalculator _metricsCalculator;
    private readonly ILogger<ProjectAnalyzer> _logger;

    public ProjectAnalyzer(IMetricsCalculator metricsCalculator, ILogger<ProjectAnalyzer> logger)
    {
        _metricsCalculator = metricsCalculator;
        _logger = logger;
    }

    public async Task<ProjectAnalysis> AnalyzeAsync(Project project, CancellationToken ct = default)
    {
        _logger.LogInformation("Analyzing project: {Name}", project.Name);

        var projectPath = project.FilePath ?? "";
        var (targetFramework, outputType, packages) = ParseProjectFile(projectPath);

        var projectReferences = project.ProjectReferences
            .Select(r => project.Solution.GetProject(r.ProjectId)?.Name ?? "unknown")
            .ToList();

        var metrics = await _metricsCalculator.CalculateAsync(project, ct);

        return new ProjectAnalysis(
            project.Name,
            projectPath,
            targetFramework,
            outputType,
            projectReferences,
            packages,
            metrics);
    }

    private (string TargetFramework, string OutputType, List<Models.PackageReference> Packages) ParseProjectFile(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
            return ("unknown", "library", []);

        try
        {
            var doc = XDocument.Load(projectPath);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

            var targetFramework = doc.Descendants(ns + "TargetFramework").FirstOrDefault()?.Value
                               ?? doc.Descendants("TargetFramework").FirstOrDefault()?.Value
                               ?? "unknown";

            var outputType = doc.Descendants(ns + "OutputType").FirstOrDefault()?.Value
                          ?? doc.Descendants("OutputType").FirstOrDefault()?.Value
                          ?? "library";

            var packages = doc.Descendants(ns + "PackageReference")
                .Concat(doc.Descendants("PackageReference"))
                .Select(e => new Models.PackageReference(
                    e.Attribute("Include")?.Value ?? "",
                    e.Attribute("Version")?.Value ?? e.Element(ns + "Version")?.Value ?? e.Element("Version")?.Value ?? ""))
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .DistinctBy(p => p.Name)
                .ToList();

            return (targetFramework, outputType, packages);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse project file: {Path}", projectPath);
            return ("unknown", "library", []);
        }
    }
}
