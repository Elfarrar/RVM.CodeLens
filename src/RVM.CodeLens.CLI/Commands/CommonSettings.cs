using System.ComponentModel;
using Spectre.Console.Cli;

namespace RVM.CodeLens.CLI.Commands;

public class CommonSettings : CommandSettings
{
    [CommandArgument(0, "<PATH>")]
    [Description("Path to .sln or .slnx file")]
    public string Path { get; set; } = "";

    [CommandOption("-f|--format")]
    [Description("Output format: json, table, markdown")]
    [DefaultValue("table")]
    public string Format { get; set; } = "table";
}

public class HotspotsSettings : CommonSettings
{
    [CommandOption("-c|--commits")]
    [Description("Number of commits to analyze")]
    [DefaultValue(100)]
    public int Commits { get; set; } = 100;
}
