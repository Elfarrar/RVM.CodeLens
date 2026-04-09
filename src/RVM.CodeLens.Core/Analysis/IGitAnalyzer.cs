using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Core.Analysis;

public interface IGitAnalyzer
{
    List<HotSpot> AnalyzeHotSpots(string repositoryPath, int commitLookback = 100);
}
