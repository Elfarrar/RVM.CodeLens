using RVM.CodeLens.CLI.Commands;
using RVM.CodeLens.Core.Workspace;
using Spectre.Console.Cli;

// Must be called before any Roslyn workspace types are loaded
MsBuildInitializer.EnsureInitialized();

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("codelens");

    config.AddCommand<AnalyzeCommand>("analyze")
        .WithDescription("Analyze a .NET solution");

    config.AddCommand<MetricsCommand>("metrics")
        .WithDescription("Show code metrics");

    config.AddCommand<DepsCommand>("deps")
        .WithDescription("Show dependency graph");

    config.AddCommand<HotspotsCommand>("hotspots")
        .WithDescription("Show hot spots (high churn + complexity)");

    config.AddCommand<ArchitectureCommand>("architecture")
        .WithDescription("Detect architecture layers and violations");
});

return await app.RunAsync(args);
