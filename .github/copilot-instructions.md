# Copilot Instructions

- This repository is a `.NET 8` CLI benchmark tool for CAD-to-PDF conversion.
- Primary goal: compare conversion performance and output behavior between:
  - `Aspose.CAD`
  - `CadLib (WoutWare)`

## Project Scope
- App type: console app using `Spectre.Console.Cli`
- Entry point: `Program.cs`
- Main commands:
  - `convert aspose`
  - `convert cadlib`
  - `benchmark`

## Build and Run
- Build: `dotnet build`
- Help: `dotnet run -- --help`
- Convert with Aspose: `dotnet run -- convert aspose -i <file.dwg> [-o output.pdf] [--license <path>] [--layout <name>]`
- Convert with CadLib: `dotnet run -- convert cadlib -i <file.dwg> [-o output.pdf]`
- Benchmark: `dotnet run -- benchmark -i <file.dwg> [-n 3] [--output-csv results.csv]`

## Code Organization
- `Converters/`
  - `IConverter.cs`
  - `AsposeCadConverter.cs`
  - `CadLibConverter.cs`
- `Benchmarking/`
  - `ConversionResult.cs`
  - `BenchmarkRunner.cs`
- `Commands/`
  - `ConvertAsposeCommand.cs`
  - `ConvertCadLibCommand.cs`
  - `BenchmarkCommand.cs`
- `Output/`
  - `TableRenderer.cs`
  - `CsvExporter.cs`

## Dependencies
- `Aspose.CAD` (`26.1.0`)
- `WW.Cad_Net8.0` (trial)
- `Spectre.Console.Cli` (`0.50.0`)

## Behavior and Constraints
- Treat this repo as a performance benchmark first.
- Preserve parity between converter flows where practical.
- Avoid unnecessary allocations and expensive repeated work in hot paths.
- Keep changes minimal and focused.
- Do not introduce new dependencies unless required.
- Validate with `dotnet build` after edits.

## Benchmark Notes
- Trial/unlicensed mode is expected (watermarks are acceptable for timing).
- Metrics tracked include wall time, CPU time, peak memory, managed memory delta, and output file size.
- `CadLib` model-space scaling uses `BoundsCalculator` + `DxfUtil.GetScaleTransform`.
