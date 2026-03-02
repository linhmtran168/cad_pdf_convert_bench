using CadBenchmark.Benchmarking;
using Spectre.Console;

namespace CadBenchmark.Output;

public static class TableRenderer
{
    public static void Render(IReadOnlyList<ConversionResult> results)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]CAD-to-PDF Benchmark Results[/]");

        table.AddColumn("Converter");
        table.AddColumn("Status");
        table.AddColumn("Iterations");
        table.AddColumn("Total Time");
        table.AddColumn("Avg Time");
        table.AddColumn("CPU Time");
        table.AddColumn("Peak Memory");
        table.AddColumn("Memory Delta");
        table.AddColumn("Output Size");

        // Find the fastest for highlighting
        var successResults = results.Where(r => r.Success).ToList();
        var fastestAvg = successResults.Count > 0
            ? successResults.Min(r => r.ElapsedTime.TotalMilliseconds / Math.Max(1, r.IterationTimes.Count))
            : 0;

        foreach (var result in results)
        {
            var iterations = result.IterationTimes.Count;
            var avgMs = iterations > 0 ? result.ElapsedTime.TotalMilliseconds / iterations : 0;
            var isFastest = result.Success && successResults.Count > 1 &&
                            Math.Abs(avgMs - fastestAvg) < 0.01;

            var nameMarkup = isFastest ? $"[green bold]{Markup.Escape(result.ConverterName)}[/]" : Markup.Escape(result.ConverterName);
            var statusMarkup = result.Success ? "[green]OK[/]" : $"[red]FAIL: {Markup.Escape(result.ErrorMessage ?? "Unknown")}[/]";

            table.AddRow(
                nameMarkup,
                statusMarkup,
                iterations.ToString(),
                FormatTime(result.ElapsedTime),
                FormatTime(TimeSpan.FromMilliseconds(avgMs)),
                FormatTime(result.CpuTime),
                FormatBytes(result.PeakMemoryBytes),
                FormatBytes(result.MemoryDeltaBytes),
                FormatBytes(result.OutputFileSizeBytes)
            );
        }

        AnsiConsole.Write(table);

        // Per-iteration breakdown
        foreach (var result in results.Where(r => r.Success && r.IterationTimes.Count > 1))
        {
            AnsiConsole.MarkupLine($"\n[bold]{Markup.Escape(result.ConverterName)}[/] per-iteration times:");
            for (int i = 0; i < result.IterationTimes.Count; i++)
            {
                AnsiConsole.MarkupLine($"  Iteration {i + 1}: {FormatTime(result.IterationTimes[i])}");
            }
        }
    }

    private static string FormatTime(TimeSpan ts)
    {
        if (ts.TotalMinutes >= 1)
            return $"{ts.TotalMinutes:F1} min";
        if (ts.TotalSeconds >= 1)
            return $"{ts.TotalSeconds:F2} s";
        return $"{ts.TotalMilliseconds:F0} ms";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "-";
        var abs = Math.Abs(bytes);
        string sign = bytes < 0 ? "-" : "";
        if (abs >= 1024 * 1024 * 1024)
            return $"{sign}{abs / (1024.0 * 1024 * 1024):F1} GB";
        if (abs >= 1024 * 1024)
            return $"{sign}{abs / (1024.0 * 1024):F1} MB";
        if (abs >= 1024)
            return $"{sign}{abs / 1024.0:F1} KB";
        return $"{sign}{abs} B";
    }
}
