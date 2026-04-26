using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RVM.CodeLens.Core.Analysis;
using RVM.CodeLens.Core.Models;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Analysis;

public class SolutionAnalyzerTests
{
    private static ProjectMetrics EmptyMetrics() => new(0, 0, 0, 0, 0, 0, 0, 0, 0, []);

    private static ProjectAnalysis MakeProject(string name, List<string>? refs = null) =>
        new(name, $"/{name}/{name}.csproj", "net10.0", "library", refs ?? [], [], EmptyMetrics());

    [Fact]
    public void SolutionAnalyzer_CanBeConstructed()
    {
        var projectAnalyzer = new Mock<IProjectAnalyzer>();
        var graphBuilder = new Mock<IDependencyGraphBuilder>();
        var architectureDetector = new Mock<IArchitectureDetector>();

        var analyzer = new SolutionAnalyzer(
            projectAnalyzer.Object,
            graphBuilder.Object,
            architectureDetector.Object,
            NullLogger<SolutionAnalyzer>.Instance);

        Assert.NotNull(analyzer);
    }

    [Fact]
    public async Task SolutionAnalyzer_ThrowsOrFails_WhenSolutionPathDoesNotExist()
    {
        var projectAnalyzer = new Mock<IProjectAnalyzer>();
        var graphBuilder = new Mock<IDependencyGraphBuilder>();
        var architectureDetector = new Mock<IArchitectureDetector>();

        graphBuilder.Setup(g => g.Build(It.IsAny<List<ProjectAnalysis>>()))
            .Returns(new DependencyGraph([], []));
        architectureDetector.Setup(a => a.Detect(It.IsAny<List<ProjectAnalysis>>()))
            .Returns(new ArchitectureAnalysis([], []));

        var analyzer = new SolutionAnalyzer(
            projectAnalyzer.Object,
            graphBuilder.Object,
            architectureDetector.Object,
            NullLogger<SolutionAnalyzer>.Instance);

        // Analyzing a non-existent solution should throw
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await analyzer.AnalyzeAsync("C:/nonexistent/solution.slnx"));
    }

    // --- SolutionAnalysis record tests ---

    [Fact]
    public void SolutionAnalysis_Record_HasCorrectProperties()
    {
        var projects = new List<ProjectAnalysis> { MakeProject("MyApp.Domain") };
        var graph = new DependencyGraph([], []);
        var arch = new ArchitectureAnalysis([], []);
        var now = DateTime.UtcNow;

        var analysis = new SolutionAnalysis("C:/sol.slnx", "sol", projects, graph, arch, now);

        Assert.Equal("C:/sol.slnx", analysis.SolutionPath);
        Assert.Equal("sol", analysis.SolutionName);
        Assert.Single(analysis.Projects);
        Assert.Equal(now, analysis.AnalyzedAt);
    }

    [Fact]
    public void SolutionAnalysis_DependencyGraph_IsIncluded()
    {
        var nodes = new List<DependencyNode> { new("MyApp.API", "MyApp.API", "project") };
        var edges = new List<DependencyEdge> { new("MyApp.API", "MyApp.Domain", "reference") };
        var graph = new DependencyGraph(nodes, edges);
        var analysis = new SolutionAnalysis("sol.slnx", "sol", [], graph, new ArchitectureAnalysis([], []), DateTime.UtcNow);

        Assert.Single(analysis.DependencyGraph.Nodes);
        Assert.Single(analysis.DependencyGraph.Edges);
    }
}
