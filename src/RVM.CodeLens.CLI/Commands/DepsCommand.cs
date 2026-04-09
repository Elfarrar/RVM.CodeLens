using RVM.CodeLens.Core.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace RVM.CodeLens.CLI.Commands;

public class DepsCommand : AsyncCommand<CommonSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        var services = AnalyzeCommand.BuildServices();
        var analyzer = services.GetRequiredService<ISolutionAnalyzer>();
        var formatter = AnalyzeCommand.GetFormatter(settings.Format);

        await AnsiConsole.Status()
            .StartAsync("Building dependency graph...", async ctx =>
            {
                var result = await analyzer.AnalyzeAsync(Path.GetFullPath(settings.Path));
                ctx.Status("Formatting output...");
                formatter.FormatDependencies(result.DependencyGraph, Console.Out);
            });

        return 0;
    }
}
