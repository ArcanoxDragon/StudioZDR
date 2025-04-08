using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui;

internal class DreadGuiCompositionDrawOperation(SpriteSheetManager spriteSheetManager) : ICustomDrawOperation
{
	private const double RenderAspectRatio = 16.0 / 9.0;

	private const int RenderPassNormal  = 0;
	private const int RenderPassOverlay = 1;
	private const int RenderPassCount   = 2;

	private static readonly SKColor HoverBorderColor    = new(0, 255, 255, 64);
	private static readonly SKColor SelectedBorderColor = new(0, 255, 255);

	private static readonly SKColor ContainerBorderColor = new(255, 0, 0);
	private static readonly SKColor SpriteBorderColor    = new(0, 255, 0);
	private static readonly SKColor TextDrawColor        = new(255, 255, 255);
	private static readonly SKColor TextBlurColor        = new(0, 0, 0);

	public DreadGuiCompositionViewModel? ViewModel { get; set; }

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
		if (ViewModel is not { Hierarchy: { } hierarchy } viewModel)
			return;

		using var guiDrawContext = new DreadGuiDrawContext(context);

		using (guiDrawContext.Canvas.WithSavedState())
		{
			var zoomFactor = (float) viewModel.ZoomFactor;
			var centerX = (float) ( RenderBounds.Width / 2 );
			var centerY = (float) ( RenderBounds.Height / 2 );

			guiDrawContext.Canvas.Translate(viewModel.PanOffset.X, viewModel.PanOffset.Y);
			guiDrawContext.Canvas.Scale(zoomFactor, zoomFactor, centerX, centerY);

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

		var objBounds = GetDisplayObjectRect(obj, parentBounds, out var origin);
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

				if (ReferenceEquals(obj, ViewModel?.SelectedObject))
					RenderDisplayObjectBounds(context, objBounds, SelectedBorderColor);
				else if (ReferenceEquals(obj, ViewModel?.HoveredObject))
					RenderDisplayObjectBounds(context, objBounds, HoverBorderColor);
			}

			if (node.IsVisible)
			{
				foreach (var childNode in node.Children)
					RenderDisplayObjectNode(context, childNode, objBounds, renderPass);
			}

			// if (!string.IsNullOrEmpty(obj.ID) && obj is GUI__CDisplayObjectContainer)
			// {
			// 	var textWidth = context.Paint.MeasureText(obj.ID);
			// 	var textX = objBounds.X + objBounds.Width * 0.5 - textWidth * 0.5;
			// 	var textY = obj switch {
			// 		// GUI__CDisplayObjectContainer => objBounds.Y + context.Paint.TextSize + 2,
			// 		_ => objBounds.Y + objBounds.Height + context.Paint.TextSize + 2,
			// 	};
			//
			// 	RenderText(context, obj.ID, textX, textY);
			// }
		}
	}

	private void RenderSprite(DreadGuiDrawContext context, GUI__CSprite sprite, Rect parentBounds)
	{
		// TODO: Visible toggle
		var spriteRect = GetDisplayObjectRect(sprite, parentBounds);
		var spriteTintColor = new SKColor(
			(byte) ( 255 * ( sprite.ColorR ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorG ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorB ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorA ?? 1.0f ) )
		);

		// RenderDisplayObjectBounds(context, sprite, spriteRect, SpriteBorderColor);

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
		var labelRect = GetDisplayObjectRect(label, parentBounds);
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

		RenderText(context, textToDraw, textX, labelRect.Y + halfHeight + TextOffset, TextSize);
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

	private Rect GetDisplayObjectRect(GUI__CDisplayObject obj, Rect parentBounds)
		=> GetDisplayObjectRect(obj, parentBounds, out _);

	private Rect GetDisplayObjectRect(GUI__CDisplayObject obj, Rect parentBounds, out Point origin)
	{
		var objWidth = ( obj.SizeX ?? 1.0 ) * RenderBounds.Width;
		var objHeight = ( obj.SizeY ?? 1.0 ) * RenderBounds.Height;
		var refX = parentBounds.X;
		var refY = parentBounds.Y;
		double boundsX, boundsY;
		double originX, originY;

		if (obj.RightX.HasValue)
		{
			var rightRefX = refX + parentBounds.Width;

			originX = rightRefX - obj.RightX.Value * RenderBounds.Width;
			boundsX = originX - objWidth;
		}
		else if (obj.CenterX.HasValue)
		{
			var centerRefX = refX + 0.5 * parentBounds.Width;

			originX = centerRefX + obj.CenterX.Value * RenderBounds.Width;
			boundsX = originX - 0.5 * objWidth;
		}
		else
		{
			originX = refX + ( obj.LeftX ?? obj.X ?? 0.0 ) * RenderBounds.Width;
			boundsX = originX;
		}

		if (obj.BottomY.HasValue)
		{
			var bottomRefY = refY + parentBounds.Height;

			originY = bottomRefY - obj.BottomY.Value * RenderBounds.Height;
			boundsY = originY - objHeight;
		}
		else if (obj.CenterY.HasValue)
		{
			var centerRefY = refY + 0.5 * parentBounds.Height;

			originY = centerRefY + obj.CenterY.Value * RenderBounds.Height;
			boundsY = originY - 0.5 * objHeight;
		}
		else
		{
			originY = refY + ( obj.TopY ?? obj.Y ?? 0.0 ) * RenderBounds.Height;
			boundsY = originY;
		}

		origin = new Point(originX, originY);
		return new Rect(boundsX, boundsY, objWidth, objHeight);
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

	private static void RenderText(DreadGuiDrawContext context, string text, double x, double y, double textSize = 14.0)
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
		context.Paint.Color = TextDrawColor;
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