# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

CAD-to-PDF Benchmark CLI tool (.NET 8) that compares Aspose.CAD and CadLib (WoutWare) conversion performance.

## Build & Run

```bash
dotnet build
dotnet run -- --help
dotnet run -- convert aspose -i <file.dwg> [-o output.pdf] [--license <path>] [--layout <name>]
dotnet run -- convert cadlib -i <file.dwg> [-o output.pdf]
dotnet run -- benchmark -i <file.dwg> [-n 3] [--output-csv results.csv]
```

## Architecture

```
CadBenchmark.csproj          # .NET 8 console app (Spectre.Console.Cli)
├── Program.cs               # CLI entry point and command wiring
├── Converters/
│   ├── IConverter.cs         # Common converter interface
│   ├── AsposeCadConverter.cs # Aspose.CAD implementation
│   └── CadLibConverter.cs   # WoutWare CadLib implementation
├── Benchmarking/
│   ├── ConversionResult.cs  # Metrics record (time, CPU, memory, file size)
│   └── BenchmarkRunner.cs   # Runs converter N iterations, captures metrics
├── Commands/
│   ├── ConvertAsposeCommand.cs  # `convert aspose` subcommand
│   ├── ConvertCadLibCommand.cs  # `convert cadlib` subcommand
│   └── BenchmarkCommand.cs     # `benchmark` subcommand
└── Output/
    ├── TableRenderer.cs     # Spectre.Console rich table output
    └── CsvExporter.cs       # CSV export of results
```

## Key Dependencies

- **Aspose.CAD** 26.1.0 — CAD to PDF conversion
- **WW.Cad_Net8.0** (trial) — CadLib CAD to PDF conversion
- **Spectre.Console.Cli** 0.50.0 — CLI framework + rich console output

## Notes

- Both libraries run in trial/unlicensed mode (watermarks in output, doesn't affect timing)
- Metrics captured: wall time, CPU time, peak memory, managed memory delta, output file size
- CadLib uses `BoundsCalculator` + `DxfUtil.GetScaleTransform` for model-space-only files
