using RVM.CodeLens.Core.Analysis;
using RVM.CodeLens.Core.Models;
using RVM.CodeLens.Web.Services;

namespace RVM.CodeLens.Web.Api;

public static class AnalysisEndpoints
{
    public record AnalyzeRequest(string SolutionPath);

    public static void MapAnalysisEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api");

        group.MapPost("/analyze", async (
            AnalyzeRequest request,
            ISolutionAnalyzer analyzer,
            IAnalysisStateService state,
            CancellationToken ct) =>
        {
            var result = await analyzer.AnalyzeAsync(request.SolutionPath, ct);
            state.SetCurrentAnalysis(result);
            return Results.Ok(result);
        });

        group.MapGet("/analysis/current", (IAnalysisStateService state) =>
        {
            var analysis = state.GetCurrentAnalysis();
            return analysis is null ? Results.NotFound() : Results.Ok(analysis);
        });

        group.MapGet("/metrics", (IAnalysisStateService state) =>
        {
            var analysis = state.GetCurrentAnalysis();
            if (analysis is null) return Results.NotFound();
            return Results.Ok(analysis.Projects.Select(p => new { p.Name, p.Metrics }));
        });

        group.MapGet("/deps", (IAnalysisStateService state) =>
        {
            var analysis = state.GetCurrentAnalysis();
            return analysis is null ? Results.NotFound() : Results.Ok(analysis.DependencyGraph);
        });

        group.MapGet("/hotspots", (IAnalysisStateService state, IGitAnalyzer gitAnalyzer) =>
        {
            var analysis = state.GetCurrentAnalysis();
            if (analysis is null) return Results.NotFound();

            var repoPath = Path.GetDirectoryName(analysis.SolutionPath)!;
            try
            {
                var hotSpots = gitAnalyzer.AnalyzeHotSpots(repoPath);
                return Results.Ok(hotSpots);
            }
            catch
            {
                return Results.Problem("Git repository not found", statusCode: 400);
            }
        });

        group.MapGet("/architecture", (IAnalysisStateService state) =>
        {
            var analysis = state.GetCurrentAnalysis();
            return analysis is null ? Results.NotFound() : Results.Ok(analysis.Architecture);
        });
    }
}
