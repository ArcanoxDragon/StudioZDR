using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.App.Extensions;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal class SpriteGridRenderer : DisplayObjectRenderer<GUI__CSpriteGrid>
{
	protected override void RenderObjectCore(DreadGuiDrawContext context, GUI__CSpriteGrid spriteGrid, GuiTransform parentTransform)
	{
		if (string.IsNullOrEmpty(spriteGrid.CellDefaultSpriteSheetItem))
			return;
		if (context.SpriteSheetManager.GetOrQueueSprite(spriteGrid.CellDefaultSpriteSheetItem) is not { } spriteBitmap)
			return;

		using var _ = context.Canvas.WithSavedState();
		var spriteColorAlpha = spriteGrid.ColorA ?? 1.0f;
		var gridBounds = spriteGrid.GetTransform(parentTransform);

		// This is "in-game" visibility, not editor visibility
		if (spriteGrid.Visible is false)
			spriteColorAlpha *= DreadGuiDrawContext.HiddenObjectAlphaMultiplier;

		var spriteTintColor = new SKColor(
			(byte) ( 255 * ( spriteGrid.ColorR ?? 1.0f ) ),
			(byte) ( 255 * ( spriteGrid.ColorG ?? 1.0f ) ),
			(byte) ( 255 * ( spriteGrid.ColorB ?? 1.0f ) ),
			(byte) ( 255 * spriteColorAlpha )
		);
		var spriteBlendMode = DreadGuiDrawContext.TranslateBlendMode(spriteGrid.BlendMode);
		using var spriteColorFilter = SKColorFilter.CreateBlendMode(spriteTintColor, SKBlendMode.Modulate);

		// Determine sprite grid dimensions - grid is centered on the object
		var cellWidth = ( spriteGrid.CellSizeX ?? 0 ) * context.RenderBounds.Width;
		var cellHeight = ( spriteGrid.CellSizeY ?? 0 ) * context.RenderBounds.Height;
		var cellSize = new Size(cellWidth, cellHeight);
		var cellCountX = spriteGrid.GridSizeX ?? 0;
		var cellCountY = spriteGrid.GridSizeY ?? 0;
		var totalGridWidth = cellWidth * cellCountX;
		var totalGridHeight = cellHeight * cellCountY;
		var gridTopLeft = gridBounds.AxisAlignedBoundingBox.Center.ToAvalonia() - new Point(totalGridWidth / 2, totalGridHeight / 2);

		context.Paint.ColorFilter = spriteColorFilter;

		for (var y = 0; y < cellCountY; y++)
		for (var x = 0; x < cellCountX; x++)
		{
			var cellOffset = new Point(x * cellWidth, y * cellHeight);
			var cellTopLeft = gridTopLeft + cellOffset;
			var cellRect = new Rect(cellTopLeft, cellSize);

			context.RenderSprite(cellRect, spriteBitmap, blendMode: spriteBlendMode);
		}

		context.Paint.ColorFilter = null;
	}
}