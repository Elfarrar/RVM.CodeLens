namespace RVM.CodeLens.Core.Models;

public record ProjectAnalysis(
    string Name,
    string Path,
    string TargetFramework,
    string OutputType,
    List<string> ProjectReferences,
    List<PackageReference> PackageReferences,
    ProjectMetrics Metrics);
