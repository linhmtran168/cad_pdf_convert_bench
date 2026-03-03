using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Drawing.Wireframe;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Tables;
using WW.Drawing;
using WW.Drawing.Printing;
using WW.Math;
using WW.Math.Geometry;

namespace CadBenchmark.Converters;

public class CadLibConverter : IConverter
{
    private readonly string? _licenseString;

    public string Name => "CadLib (WoutWare)";

    // Match Aspose.CAD page dimensions. PaperSize uses hundredths of an inch.
    private static readonly PaperSize MatchingPaperSize = new("Custom", 6297, 4723);

    private static readonly ReadConfig SharedReadConfig = new()
    {
        ReadUnknownEntityHandling = ReadUnknownEntityHandling.LoadAsUnknownEntity,
    };

    // PlotOptions wraps GraphicsConfig + paper size + plot settings
    private static readonly PlotOptions SharedPlotOptions;

    private static bool _initialized;

    static CadLibConverter()
    {
        SharedPlotOptions = new PlotOptions();
        SharedPlotOptions.GraphicsConfig.TryDrawingTextAsText = true;
        SharedPlotOptions.ModelSpacePaperSize = MatchingPaperSize;
        SharedPlotOptions.PaperSizeFallback = MatchingPaperSize;
        SharedPlotOptions.WhenHasActiveVportFitBoundsToPaperSize = true;
    }

    public CadLibConverter(string? licenseString = null)
    {
        _licenseString = licenseString;

        if (_licenseString is not null)
        {
            WW.WWLicense.SetLicense(_licenseString);
        }

        InitializeOnce();
    }

    private static void InitializeOnce()
    {
        if (_initialized) return;
        _initialized = true;

        RegisterSystemFontDirectories();
        DxfModel.FontSubstitution = new ArialUnicodeFallbackFontSubstitution();
    }

    public void Convert(string inputPath, string outputPath)
    {
        var model = CadReader.Read(inputPath, SharedReadConfig);

        var options = SharedPlotOptions;

        using var stream = new BufferedStream(File.Create(outputPath), 64 * 1024);
        var pdfExporter = new PdfExporter(stream, options);
        pdfExporter.EmbedFonts = true;

        var drawableStore = new DrawableStore();

        foreach (DxfLayout layout in model.OrderedLayouts)
        {
            AddLayoutPage(pdfExporter, options, drawableStore, model, layout);
        }

        pdfExporter.EndDocument();
    }

    private static void RegisterSystemFontDirectories()
    {
        var fontDirs = new List<string>();

        if (OperatingSystem.IsMacOS())
        {
            fontDirs.AddRange([
                "/System/Library/Fonts",
                "/System/Library/Fonts/Supplemental",
                "/Library/Fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts"),
            ]);
        }
        else if (OperatingSystem.IsLinux())
        {
            fontDirs.AddRange([
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
            ]);
        }

        var existing = fontDirs.Where(Directory.Exists).ToArray();
        if (existing.Length > 0)
        {
            DxfModel.AddGlobalShxLookupDirectories(existing);
            DxfModel.AddGlobalTrueTypeFontLookupDirectories(existing);
        }
    }

    private static void AddLayoutPage(
        PdfExporter pdfExporter,
        PlotOptions options,
        DrawableStore drawableStore,
        DxfModel model,
        DxfLayout layout)
    {
        var bounds = new Bounds3D();
        const float defaultMargin = 0.5f;
        float margin = 0f;
        PaperSize? paperSize = null;
        bool emptyLayout = false;
        Matrix4D modelTransform = Matrix4D.Identity;
        DxfVPort? activeVPort = null;
        var modelSpacePaperSize = options.ModelSpacePaperSize;

        var drawable = DrawableUtil.CreateDrawables(drawableStore, options.GraphicsConfig, model, layout);
        var drawContext = new DrawableDrawContext(model, layout, options.GraphicsConfig) { IsPlot = true };
        var drawableInstance = DrawableUtil.CreateDrawableInstance(drawContext, drawable);

        if (!layout.PaperSpace)
        {
            // Model space
            activeVPort = model.VPorts.GetActiveVPort();

            if (activeVPort != null && options.WhenHasActiveVportFitBoundsToPaperSize)
            {
                modelTransform = activeVPort.GetTransform(new Size2D(1, 1));
                activeVPort = null;
            }

            if (activeVPort != null)
            {
                paperSize = modelSpacePaperSize;
            }
            else
            {
                bounds = new Bounds3D();
                drawContext.SetPreTransform(modelTransform);
                drawable.GetWorldBounds(drawContext, bounds, false);
                drawContext.RestoreTransforms();

                if (bounds.Initialized)
                {
                    // If modelSpacePaperSize is not set, use paper size from model layout if present
                    if (modelSpacePaperSize == null && layout.PlotPaperSize != Size2D.Zero)
                    {
                        switch (layout.PlotRotation)
                        {
                            case PlotRotation.None:
                            case PlotRotation.Half:
                                modelSpacePaperSize = new PaperSize(
                                    layout.PaperSizeName,
                                    (int)Math.Round(layout.PlotPaperSize.X * 100d / 25.4d),
                                    (int)Math.Round(layout.PlotPaperSize.Y * 100d / 25.4d));
                                break;
                            default:
                                modelSpacePaperSize = new PaperSize(
                                    layout.PaperSizeName,
                                    (int)Math.Round(layout.PlotPaperSize.Y * 100d / 25.4d),
                                    (int)Math.Round(layout.PlotPaperSize.X * 100d / 25.4d));
                                break;
                        }
                        paperSize = modelSpacePaperSize;
                    }
                    else
                    {
                        paperSize = modelSpacePaperSize;
                        paperSize = EnsureHasPaperSizeAndRotateToMatchBounds(options, bounds, paperSize);
                    }
                }
                else
                {
                    emptyLayout = true;
                }
            }

            margin = defaultMargin;
        }
        else
        {
            // Paper space layout
            if (layout.PlotPaperSize == Size2D.Zero)
            {
                const double hundredthInchToMM = 25.4d / 100d;
                layout.PlotPaperSize = new Size2D(
                    options.PaperSizeFallback.Width * hundredthInchToMM,
                    options.PaperSizeFallback.Height * hundredthInchToMM);
            }

            Bounds2D plotAreaBounds = layout.GetPlotAreaBounds(options.GetPlotArea);
            bounds = new Bounds3D();
            emptyLayout = !plotAreaBounds.Initialized;

            if (plotAreaBounds.Initialized)
            {
                double customScaleFactor = 1d;
                if ((layout.PlotLayoutFlags & PlotLayoutFlags.UseStandardScale) == 0 &&
                    layout.PlotArea == PlotArea.LayoutInformation &&
                    layout.CustomPrintScaleNumerator != 0d &&
                    layout.CustomPrintScaleDenominator != 0d)
                {
                    customScaleFactor = layout.CustomPrintScaleNumerator / layout.CustomPrintScaleDenominator;
                }

                bounds.Update((Point3D)(Vector3D)((Vector2D)plotAreaBounds.Min / customScaleFactor));
                bounds.Update((Point3D)(Vector3D)((Vector2D)plotAreaBounds.Max / customScaleFactor));

                if (layout.PlotArea == PlotArea.LayoutInformation)
                {
                    switch (layout.PlotPaperUnits)
                    {
                        case PlotPaperUnits.Millimeters:
                            paperSize = new PaperSize(
                                Guid.NewGuid().ToString(),
                                (int)(plotAreaBounds.Delta.X * 100d / 25.4d),
                                (int)(plotAreaBounds.Delta.Y * 100d / 25.4d));
                            break;
                        case PlotPaperUnits.Inches:
                            paperSize = new PaperSize(
                                Guid.NewGuid().ToString(),
                                (int)(plotAreaBounds.Delta.X * 100d),
                                (int)(plotAreaBounds.Delta.Y * 100d));
                            break;
                        case PlotPaperUnits.Pixels:
                            // No physical paper units — fall back below
                            break;
                    }
                }

                if (paperSize == null)
                {
                    paperSize = EnsureHasPaperSizeAndRotateToMatchBounds(options, bounds, paperSize);
                    margin = defaultMargin;
                }
            }
        }

        if (emptyLayout || paperSize == null) return;

        float pageWidthInInches = paperSize.Width / 100f;
        float pageHeightInInches = paperSize.Height / 100f;

        Matrix4D to2DTransform;
        double scaleFactor;

        if (activeVPort != null)
        {
            to2DTransform = activeVPort.GetTransform(
                new Size2D(pageWidthInInches * PdfExporter.InchToPixel, pageHeightInInches * PdfExporter.InchToPixel),
                margin * PdfExporter.InchToPixel);
            scaleFactor = double.NaN;
        }
        else
        {
            to2DTransform = DxfUtil.GetScaleTransform(
                bounds.Corner1,
                bounds.Corner2,
                new Point3D(bounds.Center.X, bounds.Corner2.Y, 0d),
                new Point3D(new Vector3D(margin, margin, 0d) * PdfExporter.InchToPixel),
                new Point3D(new Vector3D(pageWidthInInches - margin, pageHeightInInches - margin, 0d) * PdfExporter.InchToPixel),
                new Point3D(new Vector3D(pageWidthInInches / 2d, pageHeightInInches - margin, 0d) * PdfExporter.InchToPixel),
                out scaleFactor
            ) * modelTransform;
        }

        var pageConfig = new PdfPageConfiguration(drawableInstance, model, options.GraphicsConfig, to2DTransform, paperSize)
        {
            IsPlot = drawContext.IsPlot,
        };
        if (layout.PaperSpace)
        {
            pageConfig.LayoutUnitsToPdfUnits = scaleFactor;
            pageConfig.Layout = layout;
            pageConfig.Viewports = null;
        }

        pdfExporter.DrawPage(pageConfig);
    }

    private static PaperSize EnsureHasPaperSizeAndRotateToMatchBounds(
        PlotOptions options, Bounds3D bounds, PaperSize? paperSize)
    {
        paperSize ??= options.PaperSizeFallback ?? PaperSizes.GetPaperSize(PaperKind.A4);

        if (bounds.Delta.X > bounds.Delta.Y != paperSize.Width > paperSize.Height)
        {
            paperSize = new PaperSize(paperSize.PaperName, paperSize.Height, paperSize.Width);
        }

        return paperSize;
    }
}

internal class ArialUnicodeFallbackFontSubstitution : IFontSubstitution
{
    public static readonly IList<string> FileFallbacks = ["Arial Unicode.ttf", "Arial Unicode MS.ttf"];
    public static readonly IList<string> FamilyFallbacks = ["Arial Unicode MS", "Arial Unicode"];

    public IList<string>? GetPreferredSubstitionFontFileNames(string fontFileName)
    {
        return FileFallbacks;
    }

    public IList<string>? GetPreferredSubstitionFontFamilyNames(string fontFamilyName)
    {
        return FamilyFallbacks;
    }
}
