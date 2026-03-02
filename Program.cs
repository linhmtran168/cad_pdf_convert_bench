using CadBenchmark.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("cadbench");

    config.AddBranch("convert", convert =>
    {
        convert.SetDescription("Convert a CAD file to PDF using a specific library");

        convert.AddCommand<ConvertAsposeCommand>("aspose")
            .WithDescription("Convert using Aspose.CAD");

        convert.AddCommand<ConvertCadLibCommand>("cadlib")
            .WithDescription("Convert using CadLib (WoutWare)");
    });

    config.AddCommand<BenchmarkCommand>("benchmark")
        .WithDescription("Benchmark CAD-to-PDF conversion with both libraries");
});

return app.Run(args);
