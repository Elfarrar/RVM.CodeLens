using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace RVM.CodeLens.Core.Workspace;

public class SolutionLoader : ISolutionLoader
{
    private readonly ILogger<SolutionLoader> _logger;
    private MSBuildWorkspace? _workspace;

    public SolutionLoader(ILogger<SolutionLoader> logger)
    {
        _logger = logger;
    }

    public async Task<Solution> LoadAsync(string solutionPath, CancellationToken ct = default)
    {
        MsBuildInitializer.EnsureInitialized();

        _workspace = MSBuildWorkspace.Create();
        _workspace.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                _logger.LogWarning("Workspace failure: {Message}", e.Diagnostic.Message);
            else
                _logger.LogDebug("Workspace diagnostic: {Message}", e.Diagnostic.Message);
        });

        _logger.LogInformation("Loading solution: {Path}", solutionPath);
        var solution = await _workspace.OpenSolutionAsync(solutionPath, cancellationToken: ct);
        _logger.LogInformation("Loaded {Count} projects", solution.Projects.Count());

        return solution;
    }

    public void Dispose()
    {
        _workspace?.Dispose();
    }
}
