using RVM.CodeLens.Core.Analysis;
using RVM.CodeLens.Core.Workspace;
using Microsoft.Extensions.DependencyInjection;

namespace RVM.CodeLens.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCodeLensCore(this IServiceCollection services)
    {
        services.AddTransient<ISolutionLoader, SolutionLoader>();
        services.AddTransient<ISolutionAnalyzer, SolutionAnalyzer>();
        services.AddTransient<IProjectAnalyzer, ProjectAnalyzer>();
        services.AddTransient<IMetricsCalculator, MetricsCalculator>();
        services.AddSingleton<IDependencyGraphBuilder, DependencyGraphBuilder>();
        services.AddSingleton<IArchitectureDetector, ArchitectureDetector>();
        services.AddTransient<IGitAnalyzer, GitAnalyzer>();

        return services;
    }
}
