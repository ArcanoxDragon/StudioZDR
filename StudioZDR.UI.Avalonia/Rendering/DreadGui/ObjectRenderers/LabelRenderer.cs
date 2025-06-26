using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal class LabelRenderer : DisplayObjectRenderer<GUI__CLabel>
{
	private const int TextSize   = 20;
	private const int TextOffset = 8;

	private static readonly SKColor TextDrawColor       = new(255, 255, 255);
	private static readonly SKColor HiddenTextDrawColor = new(255, 255, 255, (byte) ( 255 * DreadGuiDrawContext.HiddenObjectAlphaMultiplier ));

	protected override void RenderObjectCore(DreadGuiDrawContext context, GUI__CLabel label, Rect parentBounds)
	{
		var labelRect = label.GetDisplayObjectBounds(context.RenderBounds, parentBounds);
		var halfHeight = labelRect.Height / 2;
		var textToDraw = ( label.Text ?? label.ID ?? "GUI::CLabel" ).Replace('|', '\n');

		context.Paint.TextAlign = label.TextAlignment switch {
			"Right"    => SKTextAlign.Right,
			"Centered" => SKTextAlign.Center,
			_          => SKTextAlign.Left,
		};

		var textColor = label.Visible is true ? TextDrawColor : HiddenTextDrawColor;
		var textX = context.Paint.TextAlign switch {
			SKTextAlign.Right  => labelRect.X + labelRect.Width,
			SKTextAlign.Center => labelRect.X + ( 0.5 * labelRect.Width ),
			_                  => labelRect.X,
		};

		context.RenderText(textToDraw, textX, labelRect.Y + halfHeight + TextOffset, TextSize, textColor);
		context.Paint.TextAlign = SKTextAlign.Left;
	}
}