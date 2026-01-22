using System.Drawing;
using System.Numerics;

namespace StudioZDR.App.Features.GuiEditor.HelperTypes;

/// <summary>
/// Represents a rectangular polygon that is not necessarily axis-aligned.
/// </summary>
/// <remarks>
/// The provided points are expected to form a proper rectangular polygon, but for performance reasons,
/// this is not verified or asserted. Undefined behavior will result if the provided points do not form
/// a proper rectangle.
/// </remarks>
public struct OrientedRectangle(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
{
	public OrientedRectangle(RectangleF rectangle)
		: this(
			new Vector2(rectangle.X, rectangle.Y),
			new Vector2(rectangle.X + rectangle.Width, rectangle.Y),
			new Vector2(rectangle.X, rectangle.Y + rectangle.Height),
			new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height)
		) { }

	public Vector2 TopLeft     { get; set; } = topLeft;
	public Vector2 TopRight    { get; set; } = topRight;
	public Vector2 BottomLeft  { get; set; } = bottomLeft;
	public Vector2 BottomRight { get; set; } = bottomRight;

	public Vector2 Center => TopLeft + ( 0.5f * RightVector ) + ( 0.5f * DownVector );

	/// <summary>
	/// A <b>non-normalized</b> vector representing the direction from the rectangle's top-left corner to its top-right corner, in local space.
	/// </summary>
	public Vector2 RightVector => TopRight - TopLeft;

	/// <summary>
	/// A <b>non-normalized</b> vector representing the direction from the rectangle's top-left corner to its bottom-left corner, in local space.
	/// </summary>
	public Vector2 DownVector => BottomLeft - TopLeft;

	public float Width  => RightVector.Length();
	public float Height => DownVector.Length();

	public static implicit operator OrientedRectangle(RectangleF rectangle)
		=> new(rectangle);
}