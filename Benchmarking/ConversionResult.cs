namespace CadBenchmark.Benchmarking;

public record ConversionResult
{
    public required string ConverterName { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    public TimeSpan CpuTime { get; init; }
    public long PeakMemoryBytes { get; init; }
    public long MemoryDeltaBytes { get; init; }
    public long InputFileSizeBytes { get; init; }
    public long OutputFileSizeBytes { get; init; }
    public List<TimeSpan> IterationTimes { get; init; } = [];
}
