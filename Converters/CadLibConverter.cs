using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Math;

namespace CadBenchmark.Converters;

public class CadLibConverter : IConverter
{
    private readonly string? _licenseString;

    public string Name => "CadLib (WoutWare)";

    public CadLibConverter(string? licenseString = null)
    {
        _licenseString = licenseString;
    }

    public void Convert(string inputPath, string outputPath)
    {
        if (_licenseString is not null)
        {
            WW.WWLicense.SetLicense(_licenseString);
        }

        var model = CadReader.Read(inputPath);

        using var stream = File.Create(outputPath);
        var exporter = new PdfExporter(stream, PlotOptions.Default);

        bool hasLayouts = false;
        foreach (DxfLayout layout in model.Layouts)
        {
            if (!layout.PaperSpace)
                continue;

            hasLayouts = true;
            var config = (GraphicsConfig)GraphicsConfig.AcadLikeWithWhiteBackground.Clone();

            // Use BoundsCalculator to get layout bounds
            var boundsCalc = new BoundsCalculator();
            boundsCalc.GetBounds(model, layout);
            var bounds = boundsCalc.Bounds;

            if (bounds.Initialized)
            {
                var transform = DxfUtil.GetScaleTransform(
                    bounds.Min,
                    bounds.Max,
                    new Point3D(0, 0, 0),
                    new Point3D(bounds.Delta.X, bounds.Delta.Y, 0)
                );

                var paperSize = new WW.Drawing.Printing.PaperSize(
                    layout.Name,
                    (int)Math.Ceiling(bounds.Delta.X),
                    (int)Math.Ceiling(bounds.Delta.Y)
                );

                exporter.DrawPage(
                    model,
                    config,
                    transform,
                    1d,
                    layout,
                    layout.Viewports,
                    paperSize
                );
            }
        }

        if (!hasLayouts)
        {
            var config = (GraphicsConfig)GraphicsConfig.AcadLikeWithWhiteBackground.Clone();
            var boundsCalc = new BoundsCalculator();
            boundsCalc.GetBounds(model);
            var bounds = boundsCalc.Bounds;

            if (bounds.Initialized)
            {
                var transform = DxfUtil.GetScaleTransform(
                    bounds.Min,
                    bounds.Max,
                    new Point3D(0, 0, 0),
                    new Point3D(bounds.Delta.X, bounds.Delta.Y, 0)
                );

                var paperSize = new WW.Drawing.Printing.PaperSize(
                    "Model",
                    (int)Math.Ceiling(bounds.Delta.X),
                    (int)Math.Ceiling(bounds.Delta.Y)
                );

                exporter.DrawPage(model, config, transform, paperSize);
            }
        }

        exporter.EndDocument();
    }
}
