namespace RVM.CodeLens.Core.Models;

public record ProjectMetrics(
    int FileCount,
    int TotalLines,
    int CodeLines,
    int BlankLines,
    int CommentLines,
    int ClassCount,
    int MethodCount,
    double AverageCyclomaticComplexity,
    double AverageMaintainabilityIndex,
    List<FileMetrics> Files);

public record FileMetrics(
    string FilePath,
    int TotalLines,
    int CodeLines,
    List<TypeMetrics> Types);

public record TypeMetrics(
    string Name,
    string Kind,
    int LineCount,
    int MethodCount,
    double MaintainabilityIndex,
    List<MethodMetrics> Methods);

public record MethodMetrics(
    string Name,
    int LineCount,
    int CyclomaticComplexity,
    int ClassCoupling,
    int DepthOfInheritance);
