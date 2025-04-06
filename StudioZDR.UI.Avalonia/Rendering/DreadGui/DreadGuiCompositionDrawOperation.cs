using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui;

internal class DreadGuiCompositionDrawOperation : ICustomDrawOperation
{
	private const double RenderAspectRatio = 16.0 / 9.0;

	private static readonly SKColor ContainerBorderColor = new(255, 0, 0);
	private static readonly SKColor SpriteBorderColor    = new(0, 255, 0);
	private static readonly SKColor TextDrawColor        = new(255, 255, 255);
	private static readonly SKColor TextBlurColor        = new(0, 0, 0);

	public GUI__CDisplayObjectContainer? RootContainer { get; set; }

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
		if (RootContainer is not { } rootContainer)
			return;

		using var guiDrawContext = new DreadGuiDrawContext(context);

		RenderContainer(guiDrawContext, rootContainer, RenderBounds);
	}

	public void Dispose() { }

	public bool Equals(ICustomDrawOperation? other)
		=> other switch {
			null                                       => false,
			DreadGuiCompositionDrawOperation otherThis => ReferenceEquals(RootContainer, otherThis.RootContainer),
			_                                          => ReferenceEquals(this, other),
		};

	private void RenderDisplayObject(DreadGuiDrawContext context, GUI__CDisplayObject? obj, Rect parentBounds)
	{
		if (obj is null)
			return;

		switch (obj)
		{
			case GUI__CDisplayObjectContainer container:
				RenderContainer(context, container, parentBounds);
				break;
			case GUI__CSprite sprite:
				RenderSprite(context, sprite, parentBounds);
				break;
		}

		if (!string.IsNullOrEmpty(obj.ID) && obj is GUI__CDisplayObjectContainer or GUI__CSprite)
		{
			var objBounds = GetDisplayObjectRect(obj, parentBounds);
			var textWidth = context.Paint.MeasureText(obj.ID);
			var textX = objBounds.X + objBounds.Width * 0.5 - textWidth * 0.5;
			var textY = obj switch {
				GUI__CDisplayObjectContainer => objBounds.Y + context.Paint.TextSize + 2,
				_                            => objBounds.Y + objBounds.Height + context.Paint.TextSize + 2,
			};

			RenderText(context, obj.ID, textX, textY);
		}
	}

	private void RenderContainer(DreadGuiDrawContext context, GUI__CDisplayObjectContainer container, Rect parentBounds)
	{
		// TODO: Visible toggle
		var containerRect = GetDisplayObjectRect(container, parentBounds);

		RenderDisplayObjectBounds(context, container, containerRect, ContainerBorderColor);

		foreach (var child in container.Children)
			RenderDisplayObject(context, child, containerRect);
	}

	private void RenderSprite(DreadGuiDrawContext context, GUI__CSprite sprite, Rect parentBounds)
	{
		// TODO: Visible toggle
		// TODO: Texture
		var spriteRect = GetDisplayObjectRect(sprite, parentBounds);
		var spriteFillColor = new SKColor(
			(byte) ( 255 * ( sprite.ColorR ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorG ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorB ?? 1.0f ) ),
			(byte) ( 255 * ( sprite.ColorA ?? 1.0f ) )
		);

		RenderDisplayObjectBounds(context, sprite, spriteRect, SpriteBorderColor, spriteFillColor);
	}

	private static void RenderDisplayObjectBounds(DreadGuiDrawContext context, GUI__CDisplayObject obj, Rect rect, SKColor borderColor, SKColor? fillColor = null)
	{
		const float CornerRadius = 4.0f;

		// TODO: Push Scale, Angle

		var centerX = (float) ( rect.X + rect.Width / 2 );
		var centerY = (float) ( rect.Y + rect.Height / 2 );

		using (context.Canvas.WithSavedState())
		{
			if (obj.Angle.HasValue)
			{
				context.Canvas.Translate(centerX, centerY);
				context.Canvas.RotateRadians(obj.Angle.Value);
				context.Canvas.Translate(-centerX, -centerY);
			}

			if (fillColor.HasValue)
			{
				context.Paint.Style = SKPaintStyle.Fill;
				context.Paint.Color = fillColor.Value;
				context.Canvas.DrawRoundRect(rect.ToSKRect(), CornerRadius, CornerRadius, context.Paint);
			}

			context.Paint.Style = SKPaintStyle.Stroke;
			context.Paint.Color = borderColor;
			context.Canvas.DrawRoundRect(rect.ToSKRect(), CornerRadius, CornerRadius, context.Paint);
		}
	}

	private Rect GetDisplayObjectRect(GUI__CDisplayObject obj, Rect parentBounds)
	{
		var objWidth = ( obj.SizeX ?? 1.0 ) * RenderBounds.Width;
		var objHeight = ( obj.SizeY ?? 1.0 ) * RenderBounds.Height;
		var refX = parentBounds.X;
		var refY = parentBounds.Y;
		double objX, objY;

		if (obj.CenterX.HasValue)
		{
			var centerRefX = refX + 0.5 * parentBounds.Width;

			objX = centerRefX + obj.CenterX.Value * RenderBounds.Width - 0.5 * objWidth;
		}
		else
		{
			objX = refX + ( obj.X ?? 0.0 ) * RenderBounds.Width;
		}

		if (obj.CenterY.HasValue)
		{
			var centerRefY = refY + 0.5 * parentBounds.Height;

			objY = centerRefY + obj.CenterY.Value * RenderBounds.Height - 0.5 * objHeight;
		}
		else
		{
			objY = refY + ( obj.Y ?? 0.0 ) * RenderBounds.Height;
		}

		return new Rect(objX, objY, objWidth, objHeight);
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

	private static void RenderText(DreadGuiDrawContext context, string text, double x, double y)
	{
		// Draw text shadow for contrast (text is drawn twice for darker shadow)
		context.Paint.Style = SKPaintStyle.Fill;
		context.Paint.Color = TextBlurColor;
		context.Paint.MaskFilter = context.TextBlurFilter;
		context.Paint.FakeBoldText = true;
		context.Canvas.DrawText(text, (float) x, (float) y, context.Paint);
		context.Canvas.DrawText(text, (float) x, (float) y, context.Paint);

		// Draw normal text on top of shadow
		context.Paint.Color = TextDrawColor;
		context.Paint.MaskFilter = null;
		context.Paint.FakeBoldText = false;
		context.Canvas.DrawText(text, (float) x, (float) y, context.Paint);
	}
}