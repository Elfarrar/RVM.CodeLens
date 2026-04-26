using RVM.CodeLens.Core.Models;
using RVM.CodeLens.Web.Services;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Analysis;

public class AnalysisStateServiceTests
{
    private static SolutionAnalysis MakeAnalysis(string name = "TestSolution") =>
        new(
            $"C:/repos/{name}/{name}.slnx",
            name,
            [],
            new DependencyGraph([], []),
            new ArchitectureAnalysis([], []),
            DateTime.UtcNow);

    [Fact]
    public void GetCurrentAnalysis_ReturnsNull_Initially()
    {
        var service = new AnalysisStateService();
        Assert.Null(service.GetCurrentAnalysis());
    }

    [Fact]
    public void SetCurrentAnalysis_StoresAnalysis()
    {
        var service = new AnalysisStateService();
        var analysis = MakeAnalysis("MyApp");

        service.SetCurrentAnalysis(analysis);

        var result = service.GetCurrentAnalysis();
        Assert.NotNull(result);
        Assert.Equal("MyApp", result.SolutionName);
    }

    [Fact]
    public void SetCurrentAnalysis_OverwritesPreviousValue()
    {
        var service = new AnalysisStateService();

        service.SetCurrentAnalysis(MakeAnalysis("First"));
        service.SetCurrentAnalysis(MakeAnalysis("Second"));

        var result = service.GetCurrentAnalysis();
        Assert.NotNull(result);
        Assert.Equal("Second", result.SolutionName);
    }

    [Fact]
    public void SetAndGet_IsThreadSafe()
    {
        var service = new AnalysisStateService();
        var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() =>
        {
            service.SetCurrentAnalysis(MakeAnalysis($"Solution{i}"));
            _ = service.GetCurrentAnalysis();
        })).ToArray();

        var exception = Record.Exception(() => Task.WaitAll(tasks));
        Assert.Null(exception);
    }

    [Fact]
    public void GetCurrentAnalysis_ReturnsSameInstance()
    {
        var service = new AnalysisStateService();
        var analysis = MakeAnalysis("Stable");

        service.SetCurrentAnalysis(analysis);

        Assert.Same(analysis, service.GetCurrentAnalysis());
    }
}
