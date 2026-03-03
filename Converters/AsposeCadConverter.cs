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
        };

        if (_layoutName is not null)
        {
            rasterizationOptions.Layouts = [_layoutName];
        }

        var pdfOptions = new PdfOptions
        {
            VectorRasterizationOptions = rasterizationOptions,
        };

        rasterizationOptions.GraphicsOptions.SmoothingMode = SmoothingMode.HighQuality;
        rasterizationOptions.GraphicsOptions.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        rasterizationOptions.GraphicsOptions.InterpolationMode = InterpolationMode.HighQualityBicubic;

        image.Save(outputPath, pdfOptions);
    }
}
