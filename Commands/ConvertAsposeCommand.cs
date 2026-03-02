using System.ComponentModel;
using CadBenchmark.Converters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CadBenchmark.Commands;

public sealed class ConvertAsposeCommand : Command<ConvertAsposeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-i|--input")]
        [Description("Path to the input CAD file (DWG/DXF)")]
        public required string InputPath { get; init; }

        [CommandOption("-o|--output")]
        [Description("Path to the output PDF file (default: input name + .pdf)")]
        public string? OutputPath { get; init; }

        [CommandOption("--license")]
        [Description("Path to an Aspose.CAD license file")]
        public string? LicensePath { get; init; }

        [CommandOption("--layout")]
        [Description("Specific layout name to export")]
        public string? LayoutName { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.InputPath))
        {
            AnsiConsole.MarkupLine($"[red]Input file not found:[/] {settings.InputPath}");
            return 1;
        }

        var outputPath = settings.OutputPath
            ?? Path.ChangeExtension(settings.InputPath, ".aspose.pdf");

        var config = ConfigHelper.LoadConfig();
        var licensePath = settings.LicensePath
            ?? config["Licenses:AsposeCadLicensePath"];

        var converter = new AsposeCadConverter(licensePath, settings.LayoutName);

        AnsiConsole.MarkupLine($"[bold]Converting with {converter.Name}...[/]");
        AnsiConsole.MarkupLine($"  Input:  {settings.InputPath}");
        AnsiConsole.MarkupLine($"  Output: {outputPath}");

        try
        {
            converter.Convert(settings.InputPath, outputPath);
            var outputSize = new FileInfo(outputPath).Length;
            AnsiConsole.MarkupLine($"[green]Done![/] Output size: {outputSize:N0} bytes");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Conversion failed:[/] {ex.Message}");
            return 1;
        }
    }
}
