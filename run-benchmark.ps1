#!/usr/bin/env pwsh
# Runs benchmarks for all sample files and generates BENCHMARK.md

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot   = $PSScriptRoot
$SamplesDir = Join-Path $RepoRoot 'samples'
$OutRoot    = Join-Path $RepoRoot 'benchmark'
$ReportPath = Join-Path $RepoRoot 'BENCHMARK.md'

$Samples = Get-ChildItem -Path $SamplesDir -File |
           Where-Object { $_.Extension -in '.dwg', '.dxf' } |
           Sort-Object Name

if ($Samples.Count -eq 0) {
    Write-Error "No .dwg or .dxf files found in $SamplesDir"
    exit 1
}

function Get-CsvValue {
    param(
        [Parameter(Mandatory = $true)] $Row,
        [Parameter(Mandatory = $true)] [string[]] $Names,
        $Default = ''
    )

    foreach ($name in $Names) {
        if ($Row.PSObject.Properties.Name -contains $name) {
            return $Row.$name
        }
    }

    return $Default
}

$AllCsvRows = @()

foreach ($sample in $Samples) {
    $name   = $sample.BaseName
    $outDir = Join-Path $OutRoot $name
    $csv    = Join-Path $outDir 'results.csv'

    Write-Host "Benchmarking: $($sample.Name)" -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    Push-Location $RepoRoot
    dotnet run -- benchmark -i $sample.FullName -n 3 --output-dir $outDir --output-csv $csv
    $exitCode = $LASTEXITCODE
    Pop-Location

    if ($exitCode -ne 0) {
        Write-Warning "Benchmark failed for $($sample.Name), skipping."
        continue
    }

    if (Test-Path $csv) {
        $rows = Import-Csv $csv
        foreach ($r in $rows) {
            $elapsedMs = Get-CsvValue -Row $r -Names @('ElapsedMs', 'TotalTimeMs')
            $cpuMs = Get-CsvValue -Row $r -Names @('CpuMs', 'CpuTimeMs')
            $peakBytes = Get-CsvValue -Row $r -Names @('PeakMemoryBytes')
            $deltaBytes = Get-CsvValue -Row $r -Names @('MemoryDeltaBytes')
            $outputBytes = Get-CsvValue -Row $r -Names @('OutputFileSizeBytes')

            $AllCsvRows += [pscustomobject]@{
                Sample         = $sample.Name
                Converter      = $r.Converter
                Success        = $r.Success
                ElapsedMs      = $elapsedMs
                CpuMs          = $cpuMs
                PeakMemoryMB   = if ($peakBytes) { [math]::Round([long]$peakBytes / 1MB, 1) } else { '' }
                MemoryDeltaMB  = if ($deltaBytes) { [math]::Round([long]$deltaBytes / 1MB, 1) } else { '' }
                OutputSizeKB   = if ($outputBytes) { [math]::Round([long]$outputBytes / 1KB, 1) } else { '' }
            }
        }
    }
}

# ── Build BENCHMARK.md ──────────────────────────────────────────────────────

$date = Get-Date -Format 'yyyy-MM-dd HH:mm'

$tableRows = $AllCsvRows | ForEach-Object {
    $ok = if ($_.Success -eq 'True') { 'OK' } else { 'FAIL' }
    "| $($_.Sample) | $($_.Converter) | $ok | $($_.ElapsedMs) ms | $($_.CpuMs) ms | $($_.PeakMemoryMB) MB | $($_.MemoryDeltaMB) MB | $($_.OutputSizeKB) KB |"
}

$tableHeader = @'
| Sample | Converter | Success | Elapsed | CPU | Peak Memory | Memory Delta | Output PDF Size |
|---|---|:---:|---:|---:|---:|---:|---:|
'@

$fidelitySection = @'
## PDF Fidelity Notes

Automated pixel-diff fidelity is not implemented. Inspect outputs manually:

```
benchmark/<sample>/CadLib_WoutWare/output_0.pdf
benchmark/<sample>/Aspose.CAD/output_0.pdf
```

### Suggested fidelity checklist
- [ ] No missing text / glyphs
- [ ] Correct layout page orientation and paper size
- [ ] Similar drawing extents and scale
- [ ] Lineweights and linetypes preserved
- [ ] Hatches and fills rendered
- [ ] Viewports and clipping correct (paper-space layouts)
- [ ] No major geometry omissions
'@

$body = $tableRows -join [System.Environment]::NewLine

$report = "# CAD to PDF Benchmark Report`n`nGenerated: $date  `nIterations per converter: 3  `nMode: Release`n`n## Results`n`n$tableHeader`n$body`n`n$fidelitySection"

[System.IO.File]::WriteAllText($ReportPath, $report, [System.Text.Encoding]::UTF8)
Write-Host "`nReport written to: $ReportPath" -ForegroundColor Green
