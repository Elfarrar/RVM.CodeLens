namespace RVM.CodeLens.Core.Models;

public record ArchitectureAnalysis(
    List<ArchitectureLayer> Layers,
    List<string> Violations);

public record ArchitectureLayer(string Name, List<string> Projects);
