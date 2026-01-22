using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.App.Extensions;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal class SpriteRenderer : DisplayObjectRenderer<GUI__CSprite>
{
	protected override void RenderObjectCore(DreadGuiDrawContext context, GUI__CSprite sprite, GuiTransform parentTransform)
	{
		if (string.IsNullOrEmpty(sprite.SpriteSheetItem))
			return;

		var spriteTransform = sprite.GetTransform(parentTransform);
		var spriteCenter = spriteTransform.AxisAlignedBoundingBox.Center.ToAvalonia();
		var centerX = (float) spriteCenter.X;
		var centerY = (float) spriteCenter.Y;

		// Figure out the unrotated bounding box of the sprite (not the AABB envelope) since we will be drawing on an already-rotated canvas
		var halfSizeVec = new Vector(spriteTransform.Size.Width / 2.0, spriteTransform.Size.Height / 2.0);
		var topLeft = spriteCenter - halfSizeVec;
		var bottomRight = spriteCenter + halfSizeVec;
		var spriteRect = new Rect(topLeft, bottomRight);
		using var _ = context.Canvas.WithSavedState();

		context.Canvas.RotateRadians(spriteTransform.Angle, centerX, centerY);

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