using RVM.CodeLens.Core.Models;
using RVM.CodeLens.Core.Workspace;
using Microsoft.Extensions.Logging;

namespace RVM.CodeLens.Core.Analysis;

public class SolutionAnalyzer : ISolutionAnalyzer
{
    private readonly IProjectAnalyzer _projectAnalyzer;
    private readonly IDependencyGraphBuilder _dependencyGraphBuilder;
    private readonly IArchitectureDetector _architectureDetector;
    private readonly ILogger<SolutionAnalyzer> _logger;

    public SolutionAnalyzer(
        IProjectAnalyzer projectAnalyzer,
        IDependencyGraphBuilder dependencyGraphBuilder,
        IArchitectureDetector architectureDetector,
        ILogger<SolutionAnalyzer> logger)
    {
        _projectAnalyzer = projectAnalyzer;
        _dependencyGraphBuilder = dependencyGraphBuilder;
        _architectureDetector = architectureDetector;
        _logger = logger;
    }

    public async Task<SolutionAnalysis> AnalyzeAsync(string solutionPath, CancellationToken ct = default)
    {
        solutionPath = Path.GetFullPath(solutionPath);
        _logger.LogInformation("Starting analysis of {Solution}", solutionPath);

        using var loader = new SolutionLoader(
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
                .CreateLogger<SolutionLoader>());
        var solution = await loader.LoadAsync(solutionPath, ct);

        var projects = new List<ProjectAnalysis>();
        foreach (var project in solution.Projects)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var analysis = await _projectAnalyzer.AnalyzeAsync(project, ct);
                projects.Add(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze project: {Name}", project.Name);
            }
        }

        var dependencyGraph = _dependencyGraphBuilder.Build(projects);
        var architecture = _architectureDetector.Detect(projects);

        _logger.LogInformation("Analysis complete: {Count} projects analyzed", projects.Count);

        return new SolutionAnalysis(
            solutionPath,
            Path.GetFileNameWithoutExtension(solutionPath),
            projects,
            dependencyGraph,
            architecture,
            DateTime.UtcNow);
    }
}
