using RVM.CodeLens.Core.Models;
using Microsoft.CodeAnalysis;

namespace RVM.CodeLens.Core.Analysis;

public interface IMetricsCalculator
{
    Task<ProjectMetrics> CalculateAsync(Project project, CancellationToken ct = default);
}
