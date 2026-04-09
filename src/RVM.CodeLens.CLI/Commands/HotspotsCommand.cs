using RVM.CodeLens.Core.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace RVM.CodeLens.CLI.Commands;

public class HotspotsCommand : AsyncCommand<HotspotsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, HotspotsSettings settings)
    {
        var services = AnalyzeCommand.BuildServices();
        var gitAnalyzer = services.GetRequiredService<IGitAnalyzer>();
        var formatter = AnalyzeCommand.GetFormatter(settings.Format);

        await AnsiConsole.Status()
            .StartAsync("Analyzing git history...", async ctx =>
            {
                var repoPath = Path.GetDirectoryName(Path.GetFullPath(settings.Path))!;
                var hotSpots = await Task.Run(() => gitAnalyzer.AnalyzeHotSpots(repoPath, settings.Commits));
                ctx.Status("Formatting output...");
                formatter.FormatHotSpots(hotSpots, Console.Out);
            });

        return 0;
    }
}
