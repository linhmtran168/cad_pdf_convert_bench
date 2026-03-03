using System.Diagnostics;
using CadBenchmark.Converters;

namespace CadBenchmark.Benchmarking;

public static class BenchmarkRunner
{
    public static ConversionResult Run(IConverter converter, string inputPath, string outputDir, int iterations)
    {
        var inputFileSize = new FileInfo(inputPath).Length;
        var iterationTimes = new List<TimeSpan>(iterations);
        var totalStopwatch = new Stopwatch();
        var iterationStopwatch = new Stopwatch();

        var process = Process.GetCurrentProcess();

        // Force GC before measuring to get a clean baseline
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

        process.Refresh();
        var wsBefore = process.WorkingSet64;
        var cpuBefore = process.TotalProcessorTime;

        // Track peak working set manually (PeakWorkingSet64 is unreliable on macOS)
        long peakMemory = wsBefore;

        totalStopwatch.Start();

        string lastOutputPath = string.Empty;

        try
        {
            var converterDir = Path.Combine(
                outputDir,
                converter.Name.Replace(" ", "_").Replace("(", "").Replace(")", "")
            );
            Directory.CreateDirectory(converterDir);

            for (int i = 0; i < iterations; i++)
            {
                var outputPath = Path.Combine(converterDir, $"output_{i}.pdf");
                lastOutputPath = outputPath;

                iterationStopwatch.Restart();
                converter.Convert(inputPath, outputPath);
                iterationStopwatch.Stop();

                iterationTimes.Add(iterationStopwatch.Elapsed);

                // Sample working set after each iteration
                process.Refresh();
                var currentMemory = process.WorkingSet64;
                if (currentMemory > peakMemory) peakMemory = currentMemory;
            }

            totalStopwatch.Stop();

            process.Refresh();
            var cpuAfter = process.TotalProcessorTime;
            var wsAfter = process.WorkingSet64;
            if (wsAfter > peakMemory) peakMemory = wsAfter;

            // Memory delta = WorkingSet change (captures both managed + native allocations)
            var memoryDelta = wsAfter - wsBefore;

            var outputFileSize = File.Exists(lastOutputPath) ? new FileInfo(lastOutputPath).Length : 0;

            return new ConversionResult
            {
                ConverterName = converter.Name,
                Success = true,
                ElapsedTime = totalStopwatch.Elapsed,
                CpuTime = cpuAfter - cpuBefore,
                PeakMemoryBytes = peakMemory,
                MemoryDeltaBytes = memoryDelta,
                InputFileSizeBytes = inputFileSize,
                OutputFileSizeBytes = outputFileSize,
                IterationTimes = iterationTimes,
            };
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();

            return new ConversionResult
            {
                ConverterName = converter.Name,
                Success = false,
                ErrorMessage = ex.Message,
                ElapsedTime = totalStopwatch.Elapsed,
                InputFileSizeBytes = inputFileSize,
                IterationTimes = iterationTimes,
            };
        }
    }
}
