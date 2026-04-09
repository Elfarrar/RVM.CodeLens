using RVM.CodeLens.Core.Models;
using Microsoft.CodeAnalysis;

namespace RVM.CodeLens.Core.Analysis;

public interface IProjectAnalyzer
{
    Task<ProjectAnalysis> AnalyzeAsync(Project project, CancellationToken ct = default);
}
