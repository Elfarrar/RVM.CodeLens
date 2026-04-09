namespace RVM.CodeLens.Core.Models;

public record HotSpot(
    string FilePath,
    int CommitCount,
    int AuthorCount,
    double Complexity,
    double Score);
