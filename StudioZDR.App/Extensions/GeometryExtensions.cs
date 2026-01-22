using System.Drawing;
using System.Numerics;

namespace StudioZDR.App.Extensions;

public static class GeometryExtensions
{
	public static RectangleF GetOverallBoundingBox(this IEnumerable<RectangleF> boundingBoxes)
	{
		float left = 0, top = 0;
		float right = 0, bottom = 0;
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

		var size = new SizeF(right - left, bottom - top);

		return new RectangleF(new PointF(left, top), size);
	}

	extension(RectangleF rect)
	{
		public PointF Center       => rect.Location + ( rect.Size * 0.5f );
		public PointF TopMiddle    => rect.Center with { Y = rect.Top };
		public PointF BottomMiddle => rect.Center with { Y = rect.Bottom };
		public PointF MiddleLeft   => rect.Center with { X = rect.Left };
		public PointF MiddleRight  => rect.Center with { X = rect.Right };
	}

	extension(Vector2 vector)
	{
		public Vector2 Rotate(float radians)
		{
			var sin = Math.Sin(radians);
			var cos = Math.Cos(radians);
			var rotX = ( vector.X * cos ) - ( vector.Y * sin );
			var rotY = ( vector.X * sin ) + ( vector.Y * cos );

			return new Vector2((float) rotX, (float) rotY);
		}
	}
}