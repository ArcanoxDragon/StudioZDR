using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

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
}