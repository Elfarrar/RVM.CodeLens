using RVM.CodeLens.CLI.Formatters;
using RVM.CodeLens.Core;
using RVM.CodeLens.Core.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace RVM.CodeLens.CLI.Commands;

public class AnalyzeCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        var services = BuildServices();
        var analyzer = services.GetRequiredService<ISolutionAnalyzer>();
        var formatter = GetFormatter(settings.Format);

        await AnsiConsole.Status()
            .StartAsync("Analyzing solution...", async ctx =>
            {
                var result = await analyzer.AnalyzeAsync(System.IO.Path.GetFullPath(settings.Path));
                ctx.Status("Formatting output...");
                formatter.FormatSolution(result, Console.Out);
            });

        return 0;
    }

    internal static ServiceProvider BuildServices() =>
        new ServiceCollection()
            .AddCodeLensCore()
            .AddLogging(b => b.SetMinimumLevel(LogLevel.Warning))
            .BuildServiceProvider();

    internal static IOutputFormatter GetFormatter(string format) => format.ToLowerInvariant() switch
    {
        "json" => new JsonFormatter(),
        "markdown" or "md" => new MarkdownFormatter(),
        _ => new TableFormatter()
    };
}
