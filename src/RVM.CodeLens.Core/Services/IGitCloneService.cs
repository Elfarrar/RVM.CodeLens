namespace RVM.CodeLens.Core.Services;

public interface IGitCloneService
{
    /// <summary>
    /// Clones a public Git repository and returns the path to the discovered .sln/.slnx file.
    /// The caller should call <see cref="Cleanup"/> when done.
    /// </summary>
    Task<GitCloneResult> CloneAndDiscoverAsync(string repositoryUrl, CancellationToken ct = default);

    /// <summary>
    /// Removes the cloned repository from the temp directory.
    /// </summary>
    void Cleanup(string clonePath);
}

public record GitCloneResult(string ClonePath, string SolutionPath);
