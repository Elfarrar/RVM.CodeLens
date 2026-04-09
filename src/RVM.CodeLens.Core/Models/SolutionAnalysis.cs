namespace RVM.CodeLens.Core.Models;

public record SolutionAnalysis(
    string SolutionPath,
    string SolutionName,
    List<ProjectAnalysis> Projects,
    DependencyGraph DependencyGraph,
    ArchitectureAnalysis Architecture,
    DateTime AnalyzedAt);
