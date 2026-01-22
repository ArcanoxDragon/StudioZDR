using System.Drawing;
using Point = Avalonia.Point;

namespace StudioZDR.UI.Avalonia.Extensions;

internal static class AvaloniaGeometryExtensions
{
	extension(Point point)
	{
		public PointF ToPointF()
			=> new((float) point.X, (float) point.Y);
	}

	extension(PointF point)
	{
		public Point ToAvalonia()
			=> new(point.X, point.Y);
	}

	extension(Rect rect)
	{
		public Point TopMiddle    => rect.Center.WithY(rect.Top);
		public Point BottomMiddle => rect.Center.WithY(rect.Bottom);
		public Point MiddleLeft   => rect.Center.WithX(rect.Left);
		public Point MiddleRight  => rect.Center.WithX(rect.Right);

		public RectangleF ToRectangleF()
			=> new((float) rect.X, (float) rect.Y, (float) rect.Width, (float) rect.Height);
	}

	extension(RectangleF rect)
	{
		public Rect ToAvalonia()
			=> new(rect.X, rect.Y, rect.Width, rect.Height);
	}
}