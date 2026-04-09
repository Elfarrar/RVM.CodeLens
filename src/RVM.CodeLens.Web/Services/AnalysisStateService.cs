using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Web.Services;

public interface IAnalysisStateService
{
    SolutionAnalysis? GetCurrentAnalysis();
    void SetCurrentAnalysis(SolutionAnalysis analysis);
}

public class AnalysisStateService : IAnalysisStateService
{
    private SolutionAnalysis? _current;
    private readonly Lock _lock = new();

    public SolutionAnalysis? GetCurrentAnalysis()
    {
        lock (_lock) return _current;
    }

    public void SetCurrentAnalysis(SolutionAnalysis analysis)
    {
        lock (_lock) _current = analysis;
    }
}
