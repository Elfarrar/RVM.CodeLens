using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Core.Analysis;

public interface ISolutionAnalyzer
{
    Task<SolutionAnalysis> AnalyzeAsync(string solutionPath, CancellationToken ct = default);
}
