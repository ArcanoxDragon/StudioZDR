using MercuryEngine.Data.Types.DreadTypes;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;

internal static class DisplayObjectExtensions
{
	public static Rect GetDisplayObjectRect(this GUI__CDisplayObject obj, Rect screenBounds, Rect parentBounds)
		=> GetDisplayObjectRect(obj, screenBounds, parentBounds, out _);

	public static Rect GetDisplayObjectRect(this GUI__CDisplayObject obj, Rect screenBounds, Rect parentBounds, out Point origin)
	{
		var objWidth = ( obj.SizeX ?? 1.0 ) * screenBounds.Width;
		var objHeight = ( obj.SizeY ?? 1.0 ) * screenBounds.Height;
		var refX = parentBounds.X;
		var refY = parentBounds.Y;
		double boundsX, boundsY;
		double originX, originY;

		if (obj.RightX.HasValue)
		{
			var rightRefX = refX + parentBounds.Width;

			originX = rightRefX - obj.RightX.Value * screenBounds.Width;
			boundsX = originX - objWidth;
		}
		else if (obj.CenterX.HasValue)
		{
			var centerRefX = refX + 0.5 * parentBounds.Width;

			originX = centerRefX + obj.CenterX.Value * screenBounds.Width;
			boundsX = originX - 0.5 * objWidth;
		}
		else
		{
			originX = refX + ( obj.LeftX ?? obj.X ?? 0.0 ) * screenBounds.Width;
			boundsX = originX;
		}

		if (obj.BottomY.HasValue)
		{
			var bottomRefY = refY + parentBounds.Height;

			originY = bottomRefY - obj.BottomY.Value * screenBounds.Height;
			boundsY = originY - objHeight;
		}
		else if (obj.CenterY.HasValue)
		{
			var centerRefY = refY + 0.5 * parentBounds.Height;

			originY = centerRefY + obj.CenterY.Value * screenBounds.Height;
			boundsY = originY - 0.5 * objHeight;
		}
		else
		{
			originY = refY + ( obj.TopY ?? obj.Y ?? 0.0 ) * screenBounds.Height;
			boundsY = originY;
		}

		origin = new Point(originX, originY);
		return new Rect(boundsX, boundsY, objWidth, objHeight);
	}
}