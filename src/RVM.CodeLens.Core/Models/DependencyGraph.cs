namespace RVM.CodeLens.Core.Models;

public record DependencyGraph(
    List<DependencyNode> Nodes,
    List<DependencyEdge> Edges);

public record DependencyNode(string Id, string Label, string Type);

public record DependencyEdge(string Source, string Target, string Type);
