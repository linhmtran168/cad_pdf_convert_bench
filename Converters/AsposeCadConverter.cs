using Aspose.CAD;
using Aspose.CAD.ImageOptions;

namespace CadBenchmark.Converters;

public class AsposeCadConverter : IConverter
{
    private readonly string? _licensePath;
    private readonly string? _layoutName;

    public string Name => "Aspose.CAD";

    public AsposeCadConverter(string? licensePath = null, string? layoutName = null)
    {
        _licensePath = licensePath;
        _layoutName = layoutName;

        if (_licensePath is not null && File.Exists(_licensePath))
        {
            var license = new License();
            license.SetLicense(_licensePath);
        }
    }

    public void Convert(string inputPath, string outputPath)
    {
        using var image = Image.Load(inputPath);

        var rasterizationOptions = new CadRasterizationOptions
        {
            PageWidth = 1600,
            PageHeight = 1200,
            AutomaticLayoutsScaling = true,
            Quality = new RasterizationQuality
            {
                Arc = RasterizationQualityValue.Medium,
                Hatch = RasterizationQualityValue.Medium,
                Text = RasterizationQualityValue.Medium,
                Ole = RasterizationQualityValue.Medium,
                ObjectsPrecision = RasterizationQualityValue.Medium,
                TextThicknessNormalization = true,
            },
        };

        if (_layoutName is not null)
        {
            rasterizationOptions.Layouts = [_layoutName];
        }

        var pdfOptions = new PdfOptions
        {
            VectorRasterizationOptions = rasterizationOptions,
        };

        image.Save(outputPath, pdfOptions);
    }
}
