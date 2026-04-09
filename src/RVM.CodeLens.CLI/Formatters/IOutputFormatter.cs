using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.CLI.Formatters;

public interface IOutputFormatter
{
    void FormatSolution(SolutionAnalysis analysis, TextWriter writer);
    void FormatMetrics(SolutionAnalysis analysis, TextWriter writer);
    void FormatDependencies(DependencyGraph graph, TextWriter writer);
    void FormatHotSpots(List<HotSpot> hotSpots, TextWriter writer);
    void FormatArchitecture(ArchitectureAnalysis architecture, TextWriter writer);
}
