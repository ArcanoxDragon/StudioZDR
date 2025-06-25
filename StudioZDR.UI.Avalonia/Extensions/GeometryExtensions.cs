namespace StudioZDR.UI.Avalonia.Extensions;

internal static class GeometryExtensions
{
	public static Rect GetOverallBoundingBox(this IEnumerable<Rect> boundingBoxes)
	{
		double left = 0, top = 0;
		double right = 0, bottom = 0;
		bool isFirst = true;

		foreach (var box in boundingBoxes)
		{
			if (isFirst)
			{
				left = box.Left;
				top = box.Top;
				right = box.Right;
				bottom = box.Bottom;
			}
			else
			{
				left = Math.Min(left, box.Left);
				top = Math.Min(top, box.Top);
				right = Math.Max(right, box.Right);
				bottom = Math.Max(bottom, box.Bottom);
			}

			isFirst = false;
		}

		return new Rect(new Point(left, top), new Point(right, bottom));
	}

	extension(Rect rect)
	{
		public Point TopMiddle    => rect.Center.WithY(rect.Top);
		public Point BottomMiddle => rect.Center.WithY(rect.Bottom);
		public Point MiddleLeft   => rect.Center.WithX(rect.Left);
		public Point MiddleRight  => rect.Center.WithX(rect.Right);
	}
}