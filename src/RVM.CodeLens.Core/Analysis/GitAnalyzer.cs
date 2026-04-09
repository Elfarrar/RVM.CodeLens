using RVM.CodeLens.Core.Models;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace RVM.CodeLens.Core.Analysis;

public class GitAnalyzer : IGitAnalyzer
{
    private readonly ILogger<GitAnalyzer> _logger;

    public GitAnalyzer(ILogger<GitAnalyzer> logger)
    {
        _logger = logger;
    }

    public List<HotSpot> AnalyzeHotSpots(string repositoryPath, int commitLookback = 100)
    {
        if (!Repository.IsValid(repositoryPath))
        {
            _logger.LogWarning("Not a git repository: {Path}", repositoryPath);
            return [];
        }

        using var repo = new Repository(repositoryPath);
        var fileStats = new Dictionary<string, FileChurn>(StringComparer.OrdinalIgnoreCase);

        var commits = repo.Commits.Take(commitLookback).ToList();
        _logger.LogInformation("Analyzing {Count} commits for hot spots", commits.Count);

        foreach (var commit in commits)
        {
            var author = commit.Author.Name;

            foreach (var parent in commit.Parents)
            {
                var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
                foreach (var change in changes)
                {
                    if (!change.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!fileStats.TryGetValue(change.Path, out var churn))
                    {
                        churn = new FileChurn();
                        fileStats[change.Path] = churn;
                    }

                    churn.CommitCount++;
                    churn.Authors.Add(author);
                }
            }
        }

        // Calculate complexity for files that still exist
        var hotSpots = new List<HotSpot>();
        foreach (var (filePath, churn) in fileStats)
        {
            var fullPath = Path.Combine(repositoryPath, filePath);
            var complexity = CalculateFileComplexity(fullPath);

            var score = churn.CommitCount * Math.Max(1, complexity);
            hotSpots.Add(new HotSpot(
                filePath,
                churn.CommitCount,
                churn.Authors.Count,
                Math.Round(complexity, 2),
                Math.Round(score, 2)));
        }

        return hotSpots
            .OrderByDescending(h => h.Score)
            .Take(50)
            .ToList();
    }

    private double CalculateFileComplexity(string filePath)
    {
        if (!File.Exists(filePath)) return 0;

        try
        {
            var code = File.ReadAllText(filePath);
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var methods = root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .ToList();

            if (methods.Count == 0) return 1;

            var totalComplexity = 0;
            foreach (var method in methods)
            {
                var walker = new Roslyn.SyntaxWalkers.ComplexityWalker();
                walker.Visit(method);
                totalComplexity += walker.Complexity;
            }

            return (double)totalComplexity / methods.Count;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not calculate complexity for {File}", filePath);
            return 1;
        }
    }

    private class FileChurn
    {
        public int CommitCount { get; set; }
        public HashSet<string> Authors { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
