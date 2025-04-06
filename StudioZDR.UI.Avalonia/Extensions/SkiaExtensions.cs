using SkiaSharp;

namespace StudioZDR.UI.Avalonia.Extensions;

internal static class SkiaExtensions
{
	public static PushedSkiaState WithSavedState(this SKCanvas canvas)
	{
		var saveLevel = canvas.Save();
		return new PushedSkiaState(canvas, saveLevel);
	}

	internal readonly struct PushedSkiaState(SKCanvas canvas, int saveLevel) : IDisposable
	{
		private readonly SKCanvas canvas    = canvas;
		private readonly int      saveLevel = saveLevel;

		public void Dispose()
		{
			this.canvas.RestoreToCount(this.saveLevel);
		}
	}
}