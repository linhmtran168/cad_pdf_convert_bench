using CadBenchmark.Benchmarking;

namespace CadBenchmark.Output;

public static class CsvExporter
{
    public static void Export(IReadOnlyList<ConversionResult> results, string outputPath)
    {
        using var writer = new StreamWriter(outputPath);

        writer.WriteLine("Converter,Success,ErrorMessage,Iterations,TotalTimeMs,AvgTimeMs,CpuTimeMs,PeakMemoryBytes,MemoryDeltaBytes,InputFileSizeBytes,OutputFileSizeBytes,IterationTimesMs");

        foreach (var result in results)
        {
            var iterations = result.IterationTimes.Count;
            var avgMs = iterations > 0 ? result.ElapsedTime.TotalMilliseconds / iterations : 0;
            var iterationTimesStr = string.Join(";", result.IterationTimes.Select(t => t.TotalMilliseconds.ToString("F2")));

            writer.WriteLine(string.Join(",",
                Escape(result.ConverterName),
                result.Success,
                Escape(result.ErrorMessage ?? ""),
                iterations,
                result.ElapsedTime.TotalMilliseconds.ToString("F2"),
                avgMs.ToString("F2"),
                result.CpuTime.TotalMilliseconds.ToString("F2"),
                result.PeakMemoryBytes,
                result.MemoryDeltaBytes,
                result.InputFileSizeBytes,
                result.OutputFileSizeBytes,
                Escape(iterationTimesStr)
            ));
        }
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
