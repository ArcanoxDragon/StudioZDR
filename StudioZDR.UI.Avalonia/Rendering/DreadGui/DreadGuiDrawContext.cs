using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using StudioZDR.App.Features.GuiEditor.ViewModels;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui;

internal sealed class DreadGuiDrawContext : IDisposable
{
	#region Drawing Constants

	public const float HiddenObjectAlphaMultiplier = 0.125f;
	public const float BorderStrokeWidth           = 1;
	public const float BorderCornerRadius          = 4;

	public static readonly SKColor Black = new(0, 0, 0);
	public static readonly SKColor White = new(255, 255, 255);

	#endregion

	private readonly SpriteSheetManager spriteSheetManager;

	public DreadGuiDrawContext(SpriteSheetManager spriteSheetManager, ImmediateDrawingContext drawingContext, Rect renderBounds)
	{
		if (drawingContext.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } skiaLeaseFeature)
			throw new ApplicationException("Could not obtain Skia API lease feature!");

		this.spriteSheetManager = spriteSheetManager;

		SkiaLease = skiaLeaseFeature.Lease();
		Paint = new SKPaint {
			TextSize = 14,
			SubpixelText = true,
			IsAntialias = true,
			StrokeWidth = 2,
		};
		TextBlurFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, SKMaskFilter.ConvertRadiusToSigma(4));
		RenderBounds = renderBounds;
	}

	public ISkiaSharpApiLease  SkiaLease      { get; }
	public SKPaint             Paint          { get; }
	public SKMaskFilter        TextBlurFilter { get; }
	public Rect                RenderBounds   { get; }
	public GuiEditorViewModel? Editor         { get; init; }

	public SKCanvas Canvas => SkiaLease.SkCanvas;

	#region Render Helpers

	public void RenderDisplayObjectBounds(Rect rect, SKColor borderColor)
	{
		var zoomFactor = Editor?.ZoomFactor ?? 1.0;

		Paint.StrokeWidth = (float) ( BorderStrokeWidth / zoomFactor );
		Paint.Style = SKPaintStyle.Stroke;
		Paint.Color = borderColor;
		Canvas.DrawRoundRect(rect.ToSKRect(), BorderCornerRadius, BorderCornerRadius, Paint);
		Paint.Color = White;
	}

	public void RenderText(string text, double x, double y, double textSize = 14.0, SKColor? color = null, SKColor? shadowColor = null)
	{
		const double LineSeparation = 2;

		var lines = text.Split('\n');

		if (shadowColor.HasValue)
		{
			// Draw text shadow for contrast (text is drawn twice for darker shadow)
			Paint.Style = SKPaintStyle.Fill;
			Paint.Color = shadowColor.Value;
			Paint.MaskFilter = TextBlurFilter;
			Paint.FakeBoldText = true;
			Paint.TextSize = (float) textSize;
			DrawLines(2);
		}

		// Draw normal text on top of shadow
		Paint.Color = color ?? White;
		Paint.MaskFilter = null;
		Paint.FakeBoldText = false;
		DrawLines(1);

		void DrawLines(int count)
		{
			var lineY = y;

			foreach (var line in lines)
			{
				for (var i = 0; i < count; i++)
					Canvas.DrawText(line, (float) x, (float) lineY, Paint);

				lineY += textSize + LineSeparation;
			}
		}
	}

	public void RenderSprite(Rect spriteRect, string spriteName, SKColor? tintColor = null, SKBlendMode blendMode = SKBlendMode.SrcOver)
	{
		if (this.spriteSheetManager.GetOrQueueSprite(spriteName) is not { } spriteBitmap)
			return;

		SKColorFilter? spriteColorFilter = null;

		try
		{
			if (tintColor.HasValue)
				spriteColorFilter = SKColorFilter.CreateBlendMode(tintColor.Value, SKBlendMode.Modulate);

			Paint.ColorFilter = spriteColorFilter;
			Paint.BlendMode = blendMode;
			Canvas.DrawBitmap(spriteBitmap, spriteRect.ToSKRect(), Paint);
			Paint.ColorFilter = null;
		}
		finally
		{
			spriteColorFilter?.Dispose();
		}
	}

	private const string BlendModeAdditive   = "Additive";
	private const string BlendModeAlphaBlend = "AlphaBlend";

	public static SKBlendMode TranslateBlendMode(string? blendMode)
		=> blendMode switch {
			BlendModeAdditive => SKBlendMode.Plus,

			_ => SKBlendMode.SrcOver,
		};

	#endregion

	public void Dispose()
	{
		SkiaLease.Dispose();
		Paint.Dispose();
		TextBlurFilter.Dispose();
	}
}