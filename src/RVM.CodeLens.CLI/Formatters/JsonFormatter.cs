using System.Text.Json;
using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.CLI.Formatters;

public class JsonFormatter : IOutputFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void FormatSolution(SolutionAnalysis analysis, TextWriter writer) =>
        writer.WriteLine(JsonSerializer.Serialize(analysis, Options));

    public void FormatMetrics(SolutionAnalysis analysis, TextWriter writer) =>
        writer.WriteLine(JsonSerializer.Serialize(
            analysis.Projects.Select(p => new { p.Name, p.Metrics }), Options));

    public void FormatDependencies(DependencyGraph graph, TextWriter writer) =>
        writer.WriteLine(JsonSerializer.Serialize(graph, Options));

    public void FormatHotSpots(List<HotSpot> hotSpots, TextWriter writer) =>
        writer.WriteLine(JsonSerializer.Serialize(hotSpots, Options));

    public void FormatArchitecture(ArchitectureAnalysis architecture, TextWriter writer) =>
        writer.WriteLine(JsonSerializer.Serialize(architecture, Options));
}
