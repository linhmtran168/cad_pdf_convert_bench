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

        // Force GC before measuring
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryBefore = GC.GetTotalMemory(true);
        var cpuBefore = process.TotalProcessorTime;

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
            }

            totalStopwatch.Stop();

            process.Refresh();
            var cpuAfter = process.TotalProcessorTime;
            var peakMemory = process.PeakWorkingSet64;
            var memoryAfter = GC.GetTotalMemory(false);

            var outputFileSize = File.Exists(lastOutputPath) ? new FileInfo(lastOutputPath).Length : 0;

            return new ConversionResult
            {
                ConverterName = converter.Name,
                Success = true,
                ElapsedTime = totalStopwatch.Elapsed,
                CpuTime = cpuAfter - cpuBefore,
                PeakMemoryBytes = peakMemory,
                MemoryDeltaBytes = memoryAfter - memoryBefore,
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
