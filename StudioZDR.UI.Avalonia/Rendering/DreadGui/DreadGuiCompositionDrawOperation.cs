using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;
using Vector2 = System.Numerics.Vector2;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui;

internal class DreadGuiCompositionDrawOperation(SpriteSheetManager spriteSheetManager) : ICustomDrawOperation
{
	private const double RenderAspectRatio = 16.0 / 9.0;

	private const int RenderPassNormal  = 0;
	private const int RenderPassOverlay = 1;
	private const int RenderPassCount   = 2;

	private const float HiddenObjectAlphaMultiplier = 0.125f;

	private static readonly SKColor HoverBorderColor    = new(255, 0, 255, 64);
	private static readonly SKColor SelectedBorderColor = new(0, 255, 255);
	private static readonly SKColor TextDrawColor       = new(255, 255, 255);
	private static readonly SKColor HiddenTextDrawColor = new(255, 255, 255, (byte) ( 255 * HiddenObjectAlphaMultiplier ));
	private static readonly SKColor TextBlurColor       = new(0, 0, 0);

	public DreadGuiCompositionViewModel? Composition { get; set; }
	public GuiEditorViewModel?           Editor      { get; set; }

	public Rect Bounds
	{
		get;
		set
		{
			field = value;
			RenderBounds = GetRenderBounds(value);
		}
	}

	public Rect RenderBounds { get; private set; }

	public bool HitTest(Point p)
		=> Bounds.Contains(p);

	public void Render(ImmediateDrawingContext context)
	{
		if (Composition is not { Hierarchy: { } hierarchy })
			return;

		if (Editor is not { ZoomFactor: var zoomFactor, PanOffset: var panOffset })
		{
			zoomFactor = 1.0;
			panOffset = Vector2.Zero;
		}

		using var guiDrawContext = new DreadGuiDrawContext(context);

		using (guiDrawContext.Canvas.WithSavedState())
		{
			var centerX = (float) ( RenderBounds.Width / 2 );
			var centerY = (float) ( RenderBounds.Height / 2 );

			guiDrawContext.Canvas.Translate(panOffset.X, panOffset.Y);
			guiDrawContext.Canvas.Scale((float) zoomFactor, (float) zoomFactor, centerX, centerY);

			for (var renderPass = 0; renderPass < RenderPassCount; renderPass++)
			{
				RenderDisplayObjectNode(guiDrawContext, hierarchy, RenderBounds, renderPass);
			}
		}
	}

	public void Dispose() { }

	public bool Equals(ICustomDrawOperation? other)
		=> ReferenceEquals(other, this);

	private void RenderDisplayObjectNode(DreadGuiDrawContext context, GuiCompositionNodeViewModel? node, Rect parentBounds, int renderPass)
	{
		if (node is not { DisplayObject: { } obj })
			return;

		var objBounds = obj.GetDisplayObjectRect(RenderBounds, parentBounds, out var origin);
		var originX = (float) origin.X;
		var originY = (float) origin.Y;

		using (context.Canvas.WithSavedState())
		{
			if (obj.Angle.HasValue || obj.ScaleX.HasValue || obj.ScaleY.HasValue)
			{
				if (obj.Angle.HasValue)
					context.Canvas.RotateRadians(obj.Angle.Value, originX, originY);

				if (obj.ScaleX.HasValue || obj.ScaleY.HasValue)
					context.Canvas.Scale(obj.ScaleX ?? 1, obj.ScaleY ?? 1, originX, originY);
			}

			if (renderPass == RenderPassNormal && node.IsVisible)
			{
				switch (obj)
				{
					case GUI__CSprite sprite:
						RenderSprite(context, sprite, parentBounds);
						break;
					case GUI__CLabel label:
						RenderLabel(context, label, parentBounds);
						break;
				}
			}
			else if (renderPass == RenderPassOverlay)
			{
				context.Paint.Color = new SKColor(255, 255, 255);

				if (Editor?.SelectedObjects is { } selectedObjects && selectedObjects.Contains(obj))
					RenderDisplayObjectBounds(context, objBounds, SelectedBorderColor);
				if (ReferenceEquals(obj, Editor?.HoveredObject))
					RenderDisplayObjectBounds(context, objBounds, HoverBorderColor);
			}

			if (node.IsVisible)
			{
				foreach (var childNode in node.Children)
					RenderDisplayObjectNode(context, childNode, objBounds, renderPass);
			}
		}
	}

	private void RenderSprite(DreadGuiDrawContext context, GUI__CSprite sprite, Rect parentBounds)
	{
		var spriteRect = sprite.GetDisplayObjectRect(RenderBounds, parentBounds);
		var spriteColorAlpha = sprite.ColorA ?? 1.0f;

		// This is "in-game" visibility, not editor visibility
		if (sprite.Visible is false)
			spriteColorAlpha *= HiddenObjectAlphaMultiplier;

		var spriteTintColor = new SKColor(
			(byte) ( 255 * ( sprite.ColorR ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorG ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorB ?? 1.0f ) ),
			(byte) ( 255 * spriteColorAlpha )
		);

		using (context.Canvas.WithSavedState())
		{
			var centerX = (float) ( spriteRect.X + 0.5 * spriteRect.Width );
			var centerY = (float) ( spriteRect.Y + 0.5 * spriteRect.Height );

			if (sprite is { FlipX: true, FlipY: true })
				context.Canvas.Scale(-1, -1, centerX, centerY);
			else if (sprite.FlipX is true)
				context.Canvas.Scale(-1, 1, centerX, centerY);
			else if (sprite.FlipY is true)
				context.Canvas.Scale(1, -1, centerX, centerY);

			if (!string.IsNullOrEmpty(sprite.SpriteSheetItem) && spriteSheetManager.GetOrQueueSprite(sprite.SpriteSheetItem) is { } spriteBitmap)
			{
				using var spriteColorFilter = SKColorFilter.CreateBlendMode(spriteTintColor, SKBlendMode.Modulate);

				context.Paint.BlendMode = SKBlendMode.SrcATop;
				context.Paint.ColorFilter = spriteColorFilter;
				context.Canvas.DrawBitmap(spriteBitmap, spriteRect.ToSKRect(), context.Paint);
				context.Paint.ColorFilter = null;
			}
		}
	}

	private void RenderLabel(DreadGuiDrawContext context, GUI__CLabel label, Rect parentBounds)
	{
		const int TextSize = 20;
		const int TextOffset = 8;
		var labelRect = label.GetDisplayObjectRect(RenderBounds, parentBounds);
		var halfHeight = labelRect.Height / 2;
		var textToDraw = ( label.Text ?? label.ID ?? "GUI::CLabel" ).Replace('|', '\n');

		context.Paint.TextAlign = label.TextAlignment switch {
			"Right"    => SKTextAlign.Right,
			"Centered" => SKTextAlign.Center,
			_          => SKTextAlign.Left,
		};

		var textX = context.Paint.TextAlign switch {
			SKTextAlign.Right  => labelRect.X + labelRect.Width,
			SKTextAlign.Center => labelRect.X + 0.5 * labelRect.Width,
			_                  => labelRect.X,
		};

		RenderText(context, textToDraw, textX, labelRect.Y + halfHeight + TextOffset, TextSize, visible: label.Visible is not false);
		context.Paint.TextAlign = SKTextAlign.Left;
	}

	private static void RenderDisplayObjectBounds(DreadGuiDrawContext context, Rect rect, SKColor borderColor, SKColor? fillColor = null)
	{
		const float CornerRadius = 4.0f;

		if (fillColor.HasValue)
		{
			context.Paint.Style = SKPaintStyle.Fill;
			context.Paint.Color = fillColor.Value;
			context.Canvas.DrawRoundRect(rect.ToSKRect(), CornerRadius, CornerRadius, context.Paint);
		}

		context.Paint.Style = SKPaintStyle.Stroke;
		context.Paint.Color = borderColor;
		context.Canvas.DrawRoundRect(rect.ToSKRect(), CornerRadius, CornerRadius, context.Paint);
		context.Paint.Color = new SKColor(255, 255, 255);
	}

	private static Rect GetRenderBounds(Rect controlBounds)
	{
		var boundsAspectRatio = controlBounds.Width / controlBounds.Height;
		double renderX, renderY;
		double renderWidth, renderHeight;

		if (boundsAspectRatio > RenderAspectRatio)
		{
			// Constrained by height
			renderHeight = controlBounds.Height;
			renderWidth = renderHeight * RenderAspectRatio;
			renderX = Math.Abs(renderWidth - controlBounds.Width) / 2.0;
			renderY = 0;
		}
		else
		{
			// Constrained by width
			renderWidth = controlBounds.Width;
			renderHeight = renderWidth / RenderAspectRatio;
			renderX = 0;
			renderY = Math.Abs(renderHeight - controlBounds.Height) / 2.0;
		}

		return new Rect(renderX, renderY, renderWidth, renderHeight).Deflate(2); // Deflated to give room for root corner radius
	}

	private static void RenderText(DreadGuiDrawContext context, string text, double x, double y, double textSize = 14.0, bool visible = true)
	{
		const double LineSeparation = 2;

		var lines = text.Split('\n');

		// Draw text shadow for contrast (text is drawn twice for darker shadow)
		context.Paint.Style = SKPaintStyle.Fill;
		context.Paint.Color = TextBlurColor;
		context.Paint.MaskFilter = context.TextBlurFilter;
		context.Paint.FakeBoldText = true;
		context.Paint.TextSize = (float) textSize;
		DrawLines(2);

		// Draw normal text on top of shadow
		context.Paint.Color = visible ? TextDrawColor : HiddenTextDrawColor;
		context.Paint.MaskFilter = null;
		context.Paint.FakeBoldText = false;
		DrawLines(1);

		void DrawLines(int count)
		{
			var lineY = y;

			foreach (var line in lines)
			{
				for (var i = 0; i < count; i++)
					context.Canvas.DrawText(line, (float) x, (float) lineY, context.Paint);

				lineY += textSize + LineSeparation;
			}
		}
	}
}