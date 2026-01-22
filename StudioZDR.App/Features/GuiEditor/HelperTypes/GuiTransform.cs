using System.Drawing;
using System.Numerics;

namespace StudioZDR.App.Features.GuiEditor.HelperTypes;

public struct GuiTransform
{
	public static GuiTransform CreateDefault(RectangleF screenBounds)
		=> new() {
			ScreenBounds = screenBounds,
			Size = screenBounds.Size,
			OrientedBoundingBox = screenBounds,
			AxisAlignedBoundingBox = screenBounds,
		};

	public static GuiTransform CreateEmpty(RectangleF screenBounds)
		=> new() { ScreenBounds = screenBounds };

	public GuiTransform() { }

	public RectangleF        ScreenBounds           { get; set; }
	public PointF            Origin                 { get; set; }
	public SizeF             Size                   { get; set; }
	public float             Angle                  { get; set; }
	public Vector2           ParentRightVector      { get; set; } = Vector2.UnitX;
	public Vector2           ParentDownVector       { get; set; } = Vector2.UnitY;
	public Vector2           ObjectRightVector      { get; set; } = Vector2.UnitX;
	public Vector2           ObjectDownVector       { get; set; } = Vector2.UnitY;
	public OrientedRectangle OrientedBoundingBox    { get; set; }
	public RectangleF        AxisAlignedBoundingBox { get; set; }
}