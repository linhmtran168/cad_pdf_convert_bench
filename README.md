# CAD-to-PDF Benchmark CLI

A .NET 8 cross-platform command-line tool that benchmarks CAD-to-PDF conversion using two libraries:

- **Aspose.CAD** — commercial CAD processing library
- **CadLib (WoutWare)** — alternative CAD processing library

The tool converts DWG/DXF files to PDF and compares performance across wall time, CPU usage, memory consumption, and output file size.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Build

```bash
git clone <repo-url>
cd test_aspose_cad
dotnet build
```

## Usage

The CLI provides three commands: `convert aspose`, `convert cadlib`, and `benchmark`.

### Convert a single file with Aspose.CAD

```bash
dotnet run -- convert aspose -i drawing.dwg
dotnet run -- convert aspose -i drawing.dwg -o output.pdf --layout "Layout1"
dotnet run -- convert aspose -i drawing.dwg --license /path/to/Aspose.CAD.lic
```

| Option | Description |
|--------|-------------|
| `-i, --input` | Path to the input CAD file (DWG/DXF). Required. |
| `-o, --output` | Output PDF path. Defaults to `<input>.aspose.pdf`. |
| `--license` | Path to an Aspose.CAD license file. |
| `--layout` | Specific layout name to export. |

### Convert a single file with CadLib

```bash
dotnet run -- convert cadlib -i drawing.dwg
dotnet run -- convert cadlib -i drawing.dwg -o output.pdf
```

| Option | Description |
|--------|-------------|
| `-i, --input` | Path to the input CAD file (DWG/DXF). Required. |
| `-o, --output` | Output PDF path. Defaults to `<input>.cadlib.pdf`. |

### Benchmark both libraries

Run both converters side-by-side and display a comparison table:

```bash
dotnet run -- benchmark -i drawing.dwg
dotnet run -- benchmark -i drawing.dwg -n 5 --output-csv results.csv
dotnet run -- benchmark -i drawing.dwg --output-dir ./output --aspose-license /path/to/license.lic
```

| Option | Description |
|--------|-------------|
| `-i, --input` | Path to the input CAD file (DWG/DXF). Required. |
| `-n, --iterations` | Number of iterations per converter. Default: `3`. |
| `--output-csv` | Export results to a CSV file. |
| `--output-dir` | Directory for output PDFs. Default: temp directory. |
| `--aspose-license` | Path to an Aspose.CAD license file. |
| `--layout` | Specific layout name to export (Aspose only). |

### Benchmarked metrics

| Metric | Description |
|--------|-------------|
| Wall time | Total and per-iteration elapsed time (`Stopwatch`) |
| CPU time | `Process.TotalProcessorTime` delta before/after conversion |
| Peak memory | `Process.PeakWorkingSet64` (includes native allocations) |
| Memory delta | `GC.GetTotalMemory` managed heap change |
| Output file size | Size of the generated PDF |

### Example output

```
                    CAD-to-PDF Benchmark Results
╭───────────────────┬────────┬────────────┬────────────┬──────────┬─────────────┬──────────────┬─────────────╮
│ Converter         │ Status │ Iterations │ Total Time │ Avg Time │ CPU Time    │ Peak Memory  │ Output Size │
├───────────────────┼────────┼────────────┼────────────┼──────────┼─────────────┼──────────────┼─────────────┤
│ Aspose.CAD        │ OK     │ 3          │ 4.52 s     │ 1.51 s   │ 3.80 s      │ 512.3 MB     │ 1.2 MB      │
│ CadLib (WoutWare) │ OK     │ 3          │ 2.87 s     │ 956 ms   │ 2.40 s      │ 384.1 MB     │ 980.5 KB    │
╰───────────────────┴────────┴────────────┴────────────┴──────────┴─────────────┴──────────────┴─────────────╯
```

## Licensing notes

- **Aspose.CAD** runs in trial mode without a license file. Output PDFs will contain an evaluation watermark. Pass `--license` to use a purchased license.
- **CadLib (WW.Cad)** uses the trial NuGet package (`WW.Cad_Net8.0`). A trial license from [woutware.com](https://www.woutware.com) is required for full functionality.

Trial watermarks do not affect benchmark timing results.
