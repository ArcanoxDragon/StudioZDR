using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.UI.Avalonia.Extensions;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal class SpriteRenderer : DisplayObjectRenderer<GUI__CSprite>
{
	protected override void RenderObjectCore(DreadGuiDrawContext context, GUI__CSprite sprite, Rect parentBounds)
	{
		if (string.IsNullOrEmpty(sprite.SpriteSheetItem))
			return;

		using var _ = context.Canvas.WithSavedState();
		var spriteRect = sprite.GetDisplayObjectBounds(context.RenderBounds, parentBounds);
		var centerX = (float) ( spriteRect.X + ( 0.5 * spriteRect.Width ) );
		var centerY = (float) ( spriteRect.Y + ( 0.5 * spriteRect.Height ) );

		if (sprite is { FlipX: true, FlipY: true })
			context.Canvas.Scale(-1, -1, centerX, centerY);
		else if (sprite.FlipX is true)
			context.Canvas.Scale(-1, 1, centerX, centerY);
		else if (sprite.FlipY is true)
			context.Canvas.Scale(1, -1, centerX, centerY);

		var spriteColorAlpha = sprite.ColorA ?? 1.0f;

		// This is "in-game" visibility, not editor visibility
		if (sprite.Visible is false)
			spriteColorAlpha *= DreadGuiDrawContext.HiddenObjectAlphaMultiplier;

		var spriteTintColor = new SKColor(
			(byte) ( 255 * ( sprite.ColorR ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorG ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorB ?? 1.0f ) ),
			(byte) ( 255 * spriteColorAlpha )
		);
		var spriteBlendMode = DreadGuiDrawContext.TranslateBlendMode(sprite.BlendMode);

		context.RenderSprite(spriteRect, sprite.SpriteSheetItem, spriteTintColor, spriteBlendMode);
	}
}