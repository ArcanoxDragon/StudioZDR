using System.Drawing;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels.Properties;
using StudioZDR.App.Utility;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace StudioZDR.App.Features.GuiEditor.Extensions;

public static class DisplayObjectExtensions
{
	extension(GUI__CDisplayObject obj)
	{
		public HorizontalAnchor GetHorizontalAnchor()
			=> obj switch {
				{ RightX: not null }  => HorizontalAnchor.Right,
				{ CenterX: not null } => HorizontalAnchor.Center,
				_                     => HorizontalAnchor.Left,
			};

		public VerticalAnchor GetVerticalAnchor()
			=> obj switch {
				{ BottomY: not null } => VerticalAnchor.Bottom,
				{ CenterY: not null } => VerticalAnchor.Center,
				_                     => VerticalAnchor.Top,
			};

		public Vector4 GetColor()
			=> new(
				obj.ColorR ?? 1f,
				obj.ColorG ?? 1f,
				obj.ColorB ?? 1f,
				obj.ColorA ?? 1f
			);

		public void SetColor(Vector4? color)
		{
			if (color.HasValue)
			{
				obj.ColorR = color.Value.X;
				obj.ColorG = color.Value.Y;
				obj.ColorB = color.Value.Z;
				obj.ColorA = color.Value.W;
			}
			else
			{
				obj.ColorR = null;
				obj.ColorG = null;
				obj.ColorB = null;
				obj.ColorA = null;
			}
		}

		public GuiTransform GetTransform(GuiTransform parentTransform)
		{
			var screenBounds = parentTransform.ScreenBounds;
			var scaleX = obj.ScaleX ?? 1f;
			var scaleY = obj.ScaleY ?? 1f;
			var objWidth = ( obj.SizeX ?? 1.0f ) * screenBounds.Width * scaleX;
			var objHeight = ( obj.SizeY ?? 1.0f ) * screenBounds.Height * scaleY;
			float relRefX, relRefY;             // Relative to parent center, where 1.0 = untransformed parent size in that axis
			float relTransformX, relTransformY; // Relative to relRef, where 1.0 = screen size
			float relMinX, relMaxX;
			float relMinY, relMaxY;

			if (obj.RightX.HasValue)
			{
				relRefX = 0.5f;
				relTransformX = -obj.RightX.Value;
				relMinX = -1.0f;
				relMaxX = 0.0f;
			}
			else if (obj.CenterX.HasValue)
			{
				relRefX = 0.0f;
				relTransformX = obj.CenterX.Value;
				relMinX = -0.5f;
				relMaxX = 0.5f;
			}
			else
			{
				relRefX = -0.5f;
				relTransformX = obj.LeftX ?? obj.X ?? 0.0f;
				relMinX = 0.0f;
				relMaxX = 1.0f;
			}

			if (obj.BottomY.HasValue)
			{
				relRefY = 0.5f;
				relTransformY = -obj.BottomY.Value;
				relMinY = -1.0f;
				relMaxY = 0.0f;
			}
			else if (obj.CenterY.HasValue)
			{
				relRefY = 0.0f;
				relTransformY = obj.CenterY.Value;
				relMinY = -0.5f;
				relMaxY = 0.5f;
			}
			else
			{
				relRefY = -0.5f;
				relTransformY = obj.TopY ?? obj.Y ?? 0.0f;
				relMinY = 0.0f;
				relMaxY = 1.0f;
			}

			// Calculate oriented reference vectors
			var parentRightVector = new Vector2(1.0f, 0.0f).Rotate(parentTransform.Angle);
			var parentDownVector = new Vector2(0.0f, 1.0f).Rotate(parentTransform.Angle);
			var parentXSizeVector = parentRightVector * parentTransform.Size.Width;
			var parentYSizeVector = parentDownVector * parentTransform.Size.Height;
			var screenXSizeVector = parentRightVector * parentTransform.ScreenBounds.Width;
			var screenYSizeVector = parentDownVector * parentTransform.ScreenBounds.Height;
			var objAngle = parentTransform.Angle + ( obj.Angle ?? 0f );
			var objRightVector = new Vector2(1.0f, 0.0f).Rotate(objAngle);
			var objDownVector = new Vector2(0.0f, 1.0f).Rotate(objAngle);
			var objXSizeVector = objRightVector * objWidth;
			var objYSizeVector = objDownVector * objHeight;

			// Calculate object origin
			var relativeRef = parentTransform.AxisAlignedBoundingBox.Center.ToVector2() + ( relRefX * parentXSizeVector ) + ( relRefY * parentYSizeVector );
			var origin = relativeRef + ( relTransformX * screenXSizeVector ) + ( relTransformY * screenYSizeVector );

			// Calculate transformed corners of object
			var transformedTopLeft = origin + ( relMinX * objXSizeVector ) + ( relMinY * objYSizeVector );
			var transformedTopRight = origin + ( relMaxX * objXSizeVector ) + ( relMinY * objYSizeVector );
			var transformedBottomLeft = origin + ( relMinX * objXSizeVector ) + ( relMaxY * objYSizeVector );
			var transformedBottomRight = origin + ( relMaxX * objXSizeVector ) + ( relMaxY * objYSizeVector );
			var orientedBounds = new OrientedRectangle(transformedTopLeft, transformedTopRight, transformedBottomLeft, transformedBottomRight);

			// Calculate axis-aligned bounds from the corners
			var minX = MathHelper.MinAll(transformedTopLeft.X, transformedTopRight.X, transformedBottomLeft.X, transformedBottomRight.X);
			var maxX = MathHelper.MaxAll(transformedTopLeft.X, transformedTopRight.X, transformedBottomLeft.X, transformedBottomRight.X);
			var minY = MathHelper.MinAll(transformedTopLeft.Y, transformedTopRight.Y, transformedBottomLeft.Y, transformedBottomRight.Y);
			var maxY = MathHelper.MaxAll(transformedTopLeft.Y, transformedTopRight.Y, transformedBottomLeft.Y, transformedBottomRight.Y);
			var envelopeTopLeft = new PointF(minX, minY);
			var envelopeSize = new SizeF(maxX - minX, maxY - minY);
			var envelope = new RectangleF(envelopeTopLeft, envelopeSize);

			return new GuiTransform {
				ScreenBounds = parentTransform.ScreenBounds,
				Origin = new PointF(origin),
				Size = new SizeF(objWidth, objHeight),
				Angle = objAngle,
				ParentRightVector = parentRightVector,
				ParentDownVector = parentDownVector,
				ObjectRightVector = objRightVector,
				ObjectDownVector = objDownVector,
				OrientedBoundingBox = orientedBounds,
				AxisAlignedBoundingBox = envelope,
			};
		}

		public void MoveOriginTo(GuiTransform parentTransform, PointF origin)
		{
			var screenBounds = parentTransform.ScreenBounds;
			var parentBounds = parentTransform.OrientedBoundingBox;
			var originVec = origin.ToVector2();

			if (obj.RightX.HasValue)
			{
				// Take a vector from the parent's top right pointing to the desired origin, and project that along
				// the parent's "left vector", producing the object's inset in from the parent's right edge (in the
				// parent's local space).
				var parentTopRightToOrigin = originVec - parentBounds.TopRight;
				var parentLeftVector = -parentBounds.RightVector; // Need the "left vector" because positive RightX values move the object left
				var insetFromRight = Vector2.Dot(parentTopRightToOrigin, parentLeftVector) / parentLeftVector.Length();

				obj.RightX = insetFromRight / screenBounds.Width;
				obj.X = obj.LeftX = obj.CenterX = null;
			}
			else if (obj.CenterX.HasValue)
			{
				// Take a vector from the parent's center pointing to the desired origin, and project that along
				// the parent's "right vector", producing the X-offset of the object from its parent's center (in
				// the parent's local space).
				var parentCenterToOrigin = originVec - parentBounds.Center;
				var parentRightVector = parentBounds.RightVector;
				var xOffset = Vector2.Dot(parentCenterToOrigin, parentRightVector) / parentRightVector.Length();

				obj.CenterX = xOffset / screenBounds.Width;
				obj.X = obj.LeftX = obj.RightX = null;
			}
			else
			{
				// Take a vector from the parent's top left pointing to the desired origin, and project that along
				// the parent's "right vector", producing the X-offset of the object from its parent's left edge (in
				// the parent's local space).
				var parentTopLeftToOrigin = originVec - parentBounds.TopLeft;
				var parentRightVector = parentBounds.RightVector;
				var xOffset = Vector2.Dot(parentTopLeftToOrigin, parentRightVector) / parentRightVector.Length();

				obj.X = xOffset / screenBounds.Width;
				obj.LeftX = obj.CenterX = obj.RightX = null;
			}

			if (obj.BottomY.HasValue)
			{
				// Take a vector from the parent's bottom left pointing to the desired origin, and project that along
				// the parent's "up vector", producing the object's inset in from the parent's bottom edge (in the
				// parent's local space).
				var parentBottomLeftToOrigin = originVec - parentBounds.BottomLeft;
				var parentUpVector = -parentBounds.DownVector; // Need the "up vector" because positive BottomY values move the object left
				var insetFromBottom = Vector2.Dot(parentBottomLeftToOrigin, parentUpVector) / parentUpVector.Length();

				obj.BottomY = insetFromBottom / screenBounds.Height;
				obj.Y = obj.TopY = obj.CenterY = null;
			}
			else if (obj.CenterY.HasValue)
			{
				// Take a vector from the parent's center pointing to the desired origin, and project that along
				// the parent's "down vector", producing the Y-offset of the object from its parent's center (in
				// the parent's local space).
				var parentCenterToOrigin = originVec - parentBounds.Center;
				var parentDownVector = parentBounds.DownVector;
				var yOffset = Vector2.Dot(parentCenterToOrigin, parentDownVector) / parentDownVector.Length();

				obj.CenterY = yOffset / screenBounds.Height;
				obj.Y = obj.TopY = obj.BottomY = null;
			}
			else
			{
				// Take a vector from the parent's top left pointing to the desired origin, and project that along
				// the parent's "down vector", producing the Y-offset of the object from its parent's top edge (in
				// the parent's local space).
				var parentTopLeftToOrigin = originVec - parentBounds.TopLeft;
				var parentDownVector = parentBounds.DownVector;
				var yOffset = Vector2.Dot(parentTopLeftToOrigin, parentDownVector) / parentDownVector.Length();

				obj.Y = yOffset / screenBounds.Height;
				obj.TopY = obj.CenterY = obj.BottomY = null;
			}
		}
	}
}