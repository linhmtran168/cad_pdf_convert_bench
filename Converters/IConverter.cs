namespace CadBenchmark.Converters;

public interface IConverter
{
    string Name { get; }
    void Convert(string inputPath, string outputPath);
}
