using System.ComponentModel;
using CadBenchmark.Converters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CadBenchmark.Commands;

public sealed class ConvertCadLibCommand : Command<ConvertCadLibCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-i|--input")]
        [Description("Path to the input CAD file (DWG/DXF)")]
        public required string InputPath { get; init; }

        [CommandOption("-o|--output")]
        [Description("Path to the output PDF file (default: input name + .cadlib.pdf)")]
        public string? OutputPath { get; init; }

        [CommandOption("--license")]
        [Description("CadLib (WoutWare) license string")]
        public string? LicenseString { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.InputPath))
        {
            AnsiConsole.MarkupLine($"[red]Input file not found:[/] {settings.InputPath}");
            return 1;
        }

        var outputPath = settings.OutputPath
            ?? Path.ChangeExtension(settings.InputPath, ".cadlib.pdf");

        var config = ConfigHelper.LoadConfig();
        var licenseString = settings.LicenseString
            ?? config["Licenses:CadLibLicenseString"];

        var converter = new CadLibConverter(licenseString);

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
