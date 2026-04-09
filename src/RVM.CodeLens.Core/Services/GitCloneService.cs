using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace RVM.CodeLens.Core.Services;

public class GitCloneService(ILogger<GitCloneService> logger) : IGitCloneService
{
    public Task<GitCloneResult> CloneAndDiscoverAsync(string repositoryUrl, CancellationToken ct = default)
    {
        var repoName = ExtractRepoName(repositoryUrl);
        var clonePath = Path.Combine(Path.GetTempPath(), "codelens-clones", $"{repoName}-{Guid.NewGuid():N[..8]}");

        logger.LogInformation("Cloning {Url} to {Path}", repositoryUrl, clonePath);

        Repository.Clone(repositoryUrl, clonePath, new CloneOptions
        {
            IsBare = false,
            RecurseSubmodules = false
        });

        ct.ThrowIfCancellationRequested();

        var solutionFile = DiscoverSolutionFile(clonePath);
        if (solutionFile is null)
            throw new InvalidOperationException($"No .sln or .slnx file found in the cloned repository '{repositoryUrl}'.");

        logger.LogInformation("Discovered solution: {Solution}", solutionFile);

        return Task.FromResult(new GitCloneResult(clonePath, solutionFile));
    }

    public void Cleanup(string clonePath)
    {
        if (!Directory.Exists(clonePath)) return;

        try
        {
            // LibGit2Sharp sets some files as read-only; need to clear that first
            foreach (var file in Directory.GetFiles(clonePath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
            Directory.Delete(clonePath, true);
            logger.LogInformation("Cleaned up clone at {Path}", clonePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cleanup clone at {Path}", clonePath);
        }
    }

    private static string? DiscoverSolutionFile(string directory)
    {
        // Prefer .slnx over .sln
        var slnx = Directory.GetFiles(directory, "*.slnx", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (slnx is not null) return slnx;

        var sln = Directory.GetFiles(directory, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (sln is not null) return sln;

        // Search one level deep if not found at root
        slnx = Directory.GetFiles(directory, "*.slnx", SearchOption.AllDirectories).FirstOrDefault();
        if (slnx is not null) return slnx;

        return Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
    }

    private static string ExtractRepoName(string url)
    {
        var uri = url.TrimEnd('/');
        if (uri.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            uri = uri[..^4];
        var lastSlash = uri.LastIndexOf('/');
        return lastSlash >= 0 ? uri[(lastSlash + 1)..] : "repo";
    }
}
