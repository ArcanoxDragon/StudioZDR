using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui;

internal sealed class DreadGuiDrawContext : IDisposable
{
	public DreadGuiDrawContext(ImmediateDrawingContext drawingContext)
	{
		if (drawingContext.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } skiaLeaseFeature)
			throw new ApplicationException("Could not obtain Skia API lease feature!");

		// DrawingContext = drawingContext;
		SkiaLease = skiaLeaseFeature.Lease();
		Paint = new SKPaint {
			TextSize = 14,
			SubpixelText = true,
			IsAntialias = true,
			StrokeWidth = 2,
		};
		TextBlurFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, SKMaskFilter.ConvertRadiusToSigma(4));
	}

	public ISkiaSharpApiLease SkiaLease      { get; }
	public SKPaint            Paint          { get; }
	public SKMaskFilter       TextBlurFilter { get; }

	public SKCanvas Canvas => SkiaLease.SkCanvas;

	public void Dispose()
	{
		SkiaLease.Dispose();
		Paint.Dispose();
		TextBlurFilter.Dispose();
	}
}