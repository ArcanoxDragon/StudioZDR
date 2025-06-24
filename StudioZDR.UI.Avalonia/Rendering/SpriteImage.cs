using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace StudioZDR.UI.Avalonia.Rendering;

public class SpriteImage(SKBitmap spriteBitmap) : IImage
{
	public SKBitmap SpriteBitmap { get; } = spriteBitmap;
	public Size     Size         { get; } = new(spriteBitmap.Width, spriteBitmap.Height);

	public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
		=> context.Custom(new DrawOperation(SpriteBitmap, sourceRect, destRect));

	private sealed class DrawOperation(SKBitmap bitmap, Rect sourceRect, Rect destRect) : ICustomDrawOperation
	{
		public Rect Bounds { get; } = destRect;

		private SKBitmap Bitmap     { get; } = bitmap;
		private Rect     SourceRect { get; } = sourceRect;

		public bool HitTest(Point p)
			=> Bounds.Contains(p);

		public void Render(ImmediateDrawingContext context)
		{
			if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } skiaFeature)
				return;

			using var skia = skiaFeature.Lease();

			skia.SkCanvas.DrawBitmap(Bitmap, SourceRect.ToSKRect(), Bounds.ToSKRect());
		}

		public bool Equals(ICustomDrawOperation? other)
			=> other is DrawOperation otherOperation && ReferenceEquals(Bitmap, otherOperation.Bitmap);

		public void Dispose() { }
	}
}