using RVM.CodeLens.Core.Analysis;
using RVM.CodeLens.Core.Models;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Analysis;

public class DependencyGraphBuilderTests
{
    private readonly DependencyGraphBuilder _builder = new();

    [Fact]
    public void Should_Create_Nodes_For_Projects()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.API", [], []),
            CreateProject("MyApp.Domain", [], [])
        };

        var graph = _builder.Build(projects);

        Assert.Equal(2, graph.Nodes.Count(n => n.Type == "project"));
        Assert.Contains(graph.Nodes, n => n.Id == "MyApp.API");
        Assert.Contains(graph.Nodes, n => n.Id == "MyApp.Domain");
    }

    [Fact]
    public void Should_Create_Edges_For_Project_References()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.API", ["MyApp.Domain"], []),
            CreateProject("MyApp.Domain", [], [])
        };

        var graph = _builder.Build(projects);

        Assert.Contains(graph.Edges, e =>
            e.Source == "MyApp.API" && e.Target == "MyApp.Domain" && e.Type == "reference");
    }

    [Fact]
    public void Should_Create_Nodes_And_Edges_For_Packages()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp", [], [new("Newtonsoft.Json", "13.0.3")])
        };

        var graph = _builder.Build(projects);

        Assert.Contains(graph.Nodes, n => n.Type == "package" && n.Id == "pkg:Newtonsoft.Json");
        Assert.Contains(graph.Edges, e =>
            e.Source == "MyApp" && e.Target == "pkg:Newtonsoft.Json" && e.Type == "package");
    }

    [Fact]
    public void Should_Deduplicate_Shared_Packages()
    {
        var sharedPackage = new PackageReference("Serilog", "3.0.0");
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("ProjectA", [], [sharedPackage]),
            CreateProject("ProjectB", [], [sharedPackage])
        };

        var graph = _builder.Build(projects);

        var packageNodes = graph.Nodes.Where(n => n.Id == "pkg:Serilog").ToList();
        Assert.Single(packageNodes);

        var packageEdges = graph.Edges.Where(e => e.Target == "pkg:Serilog").ToList();
        Assert.Equal(2, packageEdges.Count);
    }

    private static ProjectAnalysis CreateProject(
        string name, List<string> projectRefs, List<PackageReference> packages) =>
        new(name, $"src/{name}/{name}.csproj", "net10.0", "library",
            projectRefs, packages,
            new ProjectMetrics(0, 0, 0, 0, 0, 0, 0, 0, 100, []));
}
