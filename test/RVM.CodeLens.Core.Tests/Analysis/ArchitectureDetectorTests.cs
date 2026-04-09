using RVM.CodeLens.Core.Analysis;
using RVM.CodeLens.Core.Models;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Analysis;

public class ArchitectureDetectorTests
{
    private readonly ArchitectureDetector _detector = new();

    [Fact]
    public void Should_Detect_Standard_Layers()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.API", []),
            CreateProject("MyApp.Domain", []),
            CreateProject("MyApp.Infrastructure", []),
            CreateProject("MyApp.Tests", [])
        };

        var result = _detector.Detect(projects);

        Assert.Contains(result.Layers, l => l.Name == "Presentation" && l.Projects.Contains("MyApp.API"));
        Assert.Contains(result.Layers, l => l.Name == "Domain" && l.Projects.Contains("MyApp.Domain"));
        Assert.Contains(result.Layers, l => l.Name == "Infrastructure" && l.Projects.Contains("MyApp.Infrastructure"));
        Assert.Contains(result.Layers, l => l.Name == "Tests" && l.Projects.Contains("MyApp.Tests"));
    }

    [Fact]
    public void Should_Detect_No_Violations_In_Clean_Architecture()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.API", ["MyApp.Domain", "MyApp.Infrastructure"]),
            CreateProject("MyApp.Infrastructure", ["MyApp.Domain"]),
            CreateProject("MyApp.Domain", [])
        };

        var result = _detector.Detect(projects);

        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Should_Detect_Domain_Referencing_Infrastructure_As_Violation()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.Domain", ["MyApp.Infrastructure"]),
            CreateProject("MyApp.Infrastructure", [])
        };

        var result = _detector.Detect(projects);

        Assert.NotEmpty(result.Violations);
        Assert.Contains(result.Violations, v => v.Contains("MyApp.Domain") && v.Contains("MyApp.Infrastructure"));
    }

    [Fact]
    public void Should_Detect_Domain_Referencing_API_As_Violation()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.Domain", ["MyApp.API"]),
            CreateProject("MyApp.API", [])
        };

        var result = _detector.Detect(projects);

        Assert.NotEmpty(result.Violations);
    }

    [Fact]
    public void Tests_Can_Reference_Anything_Without_Violations()
    {
        var projects = new List<ProjectAnalysis>
        {
            CreateProject("MyApp.Tests", ["MyApp.API", "MyApp.Domain", "MyApp.Infrastructure"]),
            CreateProject("MyApp.API", []),
            CreateProject("MyApp.Domain", []),
            CreateProject("MyApp.Infrastructure", [])
        };

        var result = _detector.Detect(projects);

        Assert.Empty(result.Violations);
    }

    private static ProjectAnalysis CreateProject(string name, List<string> projectRefs) =>
        new(name, $"src/{name}/{name}.csproj", "net10.0", "library",
            projectRefs, [],
            new ProjectMetrics(0, 0, 0, 0, 0, 0, 0, 0, 100, []));
}
