using System.ComponentModel;
using CadBenchmark.Benchmarking;
using CadBenchmark.Converters;
using CadBenchmark.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CadBenchmark.Commands;

public sealed class BenchmarkCommand : Command<BenchmarkCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-i|--input")]
        [Description("Path to the input CAD file (DWG/DXF)")]
        public required string InputPath { get; init; }

        [CommandOption("-n|--iterations")]
        [Description("Number of iterations per converter (default: 3)")]
        [DefaultValue(3)]
        public int Iterations { get; init; }

        [CommandOption("--output-csv")]
        [Description("Path to export results as CSV")]
        public string? OutputCsvPath { get; init; }

        [CommandOption("--output-dir")]
        [Description("Directory for output PDF files (default: temp directory)")]
        public string? OutputDir { get; init; }

        [CommandOption("--aspose-license")]
        [Description("Path to an Aspose.CAD license file")]
        public string? AsposeLicensePath { get; init; }

        [CommandOption("--layout")]
        [Description("Specific layout name to export (Aspose only)")]
        public string? LayoutName { get; init; }

        [CommandOption("--cadlib-license")]
        [Description("CadLib (WoutWare) license string")]
        public string? CadLibLicenseString { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.InputPath))
        {
            AnsiConsole.MarkupLine($"[red]Input file not found:[/] {settings.InputPath}");
            return 1;
        }

        var outputDir = settings.OutputDir ?? Path.Combine(Directory.GetCurrentDirectory(), "benchmark");
        Directory.CreateDirectory(outputDir);

        AnsiConsole.MarkupLine($"[bold]CAD-to-PDF Benchmark[/]");
        AnsiConsole.MarkupLine($"  Input:      {settings.InputPath}");
        AnsiConsole.MarkupLine($"  Iterations: {settings.Iterations}");
        AnsiConsole.MarkupLine($"  Output dir: {outputDir}");
        AnsiConsole.WriteLine();

        var config = ConfigHelper.LoadConfig();
        var asposeLicensePath = settings.AsposeLicensePath
            ?? config["Licenses:AsposeCadLicensePath"];
        var cadLibLicenseString = settings.CadLibLicenseString
            ?? config["Licenses:CadLibLicenseString"];

        var converters = new IConverter[]
        {
            new AsposeCadConverter(asposeLicensePath, settings.LayoutName),
            new CadLibConverter(cadLibLicenseString),
        };

        var results = new List<ConversionResult>();

        foreach (var converter in converters)
        {
            AnsiConsole.MarkupLine($"Running [bold]{Markup.Escape(converter.Name)}[/]...");
            var result = BenchmarkRunner.Run(converter, settings.InputPath, outputDir, settings.Iterations);
            results.Add(result);
        }

        AnsiConsole.WriteLine();
        TableRenderer.Render(results);

        if (settings.OutputCsvPath is not null)
        {
            CsvExporter.Export(results, settings.OutputCsvPath);
            AnsiConsole.MarkupLine($"\n[green]CSV exported to:[/] {settings.OutputCsvPath}");
        }

        return 0;
    }
}
