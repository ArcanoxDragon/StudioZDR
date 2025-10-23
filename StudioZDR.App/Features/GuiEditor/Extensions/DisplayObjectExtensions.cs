using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels.Properties;
using Vector4 = System.Numerics.Vector4;

namespace StudioZDR.App.Features.GuiEditor.Extensions;

public static class DisplayObjectExtensions
{
	public static HorizontalAnchor GetHorizontalAnchor(this GUI__CDisplayObject displayObject)
		=> displayObject switch {
			{ RightX: not null }  => HorizontalAnchor.Right,
			{ CenterX: not null } => HorizontalAnchor.Center,
			_                     => HorizontalAnchor.Left,
		};

	public static VerticalAnchor GetVerticalAnchor(this GUI__CDisplayObject displayObject)
		=> displayObject switch {
			{ BottomY: not null } => VerticalAnchor.Bottom,
			{ CenterY: not null } => VerticalAnchor.Center,
			_                     => VerticalAnchor.Top,
		};

	public static Vector4 GetColor(this GUI__CDisplayObject displayObject)
		=> new(
			displayObject.ColorR ?? 1f,
			displayObject.ColorG ?? 1f,
			displayObject.ColorB ?? 1f,
			displayObject.ColorA ?? 1f
		);

	public static void SetColor(this GUI__CDisplayObject displayObject, Vector4? color)
	{
		if (color.HasValue)
		{
			displayObject.ColorR = color.Value.X;
			displayObject.ColorG = color.Value.Y;
			displayObject.ColorB = color.Value.Z;
			displayObject.ColorA = color.Value.W;
		}
		else
		{
			displayObject.ColorR = null;
			displayObject.ColorG = null;
			displayObject.ColorB = null;
			displayObject.ColorA = null;
		}
	}
}