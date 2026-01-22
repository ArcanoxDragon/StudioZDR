using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using MercuryEngine.Data.Types.DreadTypes;
using SkiaSharp;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;
using StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;
using Vector2 = System.Numerics.Vector2;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui;

internal class DreadGuiCompositionDrawOperation(SpriteSheetManager spriteSheetManager) : ICustomDrawOperation
{
	private const double RenderAspectRatio = 16.0 / 9.0;

	private const int RenderPassNormal  = 0;
	private const int RenderPassOverlay = 1;
	private const int RenderPassCount   = 2;

	private const float SelectionBoxStrokeWidth = 0; // Hairline
	private const float SelectionBoxDashLength  = 5;
	private const float SelectionHandleSize     = 10;

	private static readonly float[] SelectionBoxDashIntervals = [SelectionBoxDashLength, SelectionBoxDashLength];

	private static readonly SKColor Black               = new(0, 0, 0);
	private static readonly SKColor White               = new(255, 255, 255);
	private static readonly SKColor HoverBorderColor    = new(255, 0, 255, 64);
	private static readonly SKColor SelectedBorderColor = new(0, 255, 255);

	private static readonly Dictionary<Type, IDisplayObjectRenderer> DisplayObjectRenderers = new() {
		{ typeof(GUI__CLabel), new LabelRenderer() },
		{ typeof(GUI__CSprite), new SpriteRenderer() },
		{ typeof(GUI__CSpriteGrid), new SpriteGridRenderer() },
	};

	private readonly ManualResetEventSlim renderingEvent = new(true);

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
		this.renderingEvent.Reset();

		try
		{
			if (Composition is not { Hierarchy: var hierarchy } composition)
				return;

			using var _ = composition.LockForReading();

			if (Editor is not { ZoomFactor: var zoomFactor, PanOffset: var panOffset })
			{
				zoomFactor = 1.0;
				panOffset = Vector2.Zero;
			}

			using var guiDrawContext = new DreadGuiDrawContext(spriteSheetManager, context, RenderBounds) {
				Editor = Editor,
			};

			using (guiDrawContext.Canvas.WithSavedState())
			{
				var renderBoundsF = RenderBounds.ToRectangleF();
				var rootBounds = GuiTransform.CreateDefault(renderBoundsF);
				var centerX = renderBoundsF.Width / 2f;
				var centerY = renderBoundsF.Height / 2f;

				guiDrawContext.Canvas.Translate(panOffset.X, panOffset.Y);
				guiDrawContext.Canvas.Scale((float) zoomFactor, (float) zoomFactor, centerX, centerY);

				for (var renderPass = 0; renderPass < RenderPassCount; renderPass++)
				{
					RenderDisplayObjectNode(guiDrawContext, hierarchy, rootBounds, renderPass);

					if (renderPass == RenderPassOverlay && Editor is { SelectedNodes.Count: > 0 })
					{
						var overallSelectionBounds = Editor.SelectedNodes.GetOverallBounds(renderBoundsF);

						RenderSelectionBox(guiDrawContext, overallSelectionBounds.ToAvalonia());
					}
				}
			}
		}
		finally
		{
			this.renderingEvent.Set();
		}
	}

	public void WaitForRendering()
		=> this.renderingEvent.Wait();

	public void Dispose()
	{
		// ICustomDrawOperation.Dispose is called after every time it is rendered!
		// This class is *reused* between renders. We can't actually dispose here.
	}

	public void DisposeFinal()
	{
		// This is called when the consumer of this draw operation is actually FINISHED with
		// this instance. We can't use the real IDisposable contract because of the behavior
		// explained in the Dispose() method.
		this.renderingEvent.Dispose();
	}

	public bool Equals(ICustomDrawOperation? other)
		=> ReferenceEquals(other, this);

	private void RenderDisplayObjectNode(DreadGuiDrawContext context, GuiCompositionNodeViewModel? node, GuiTransform parentTransform, int renderPass)
	{
		if (node is not { DisplayObject: { } obj })
			return;

		var objBounds = obj.GetTransform(parentTransform);

		using (context.Canvas.WithSavedState())
		{
			if (renderPass == RenderPassNormal && node.IsVisible)
			{
				if (TryGetRenderer(obj, out var renderer))
					renderer.RenderObject(context, obj, parentTransform);
			}
			else if (renderPass == RenderPassOverlay)
			{
				context.Paint.Color = White;

				var aabb = objBounds.AxisAlignedBoundingBox.ToAvalonia();

				if (Editor?.SelectedObjects is { } selectedObjects && selectedObjects.Contains(obj))
					context.RenderDisplayObjectBounds(aabb, SelectedBorderColor);
				if (ReferenceEquals(obj, Editor?.HoveredObject))
					context.RenderDisplayObjectBounds(aabb, HoverBorderColor);
			}
		}

		if (node.IsVisible)
		{
			foreach (var childNode in node.Children)
				RenderDisplayObjectNode(context, childNode, objBounds, renderPass);
		}
	}

	private void RenderSelectionBox(DreadGuiDrawContext context, Rect rect)
	{
		using var blackDashEffect = SKPathEffect.CreateDash(SelectionBoxDashIntervals, 0);
		using var whiteDashEffect = SKPathEffect.CreateDash(SelectionBoxDashIntervals, SelectionBoxDashLength);

		// Draw black dashes
		context.Paint.Style = SKPaintStyle.Stroke;
		context.Paint.StrokeWidth = SelectionBoxStrokeWidth;
		context.Paint.PathEffect = blackDashEffect;
		context.Paint.Color = Black;
		context.Canvas.DrawRect(rect.ToSKRect(), context.Paint);

		// Draw white dashes
		context.Paint.PathEffect = whiteDashEffect;
		context.Paint.Color = White;
		context.Canvas.DrawRect(rect.ToSKRect(), context.Paint);

		// Draw selection handles
		var zoomFactor = Editor?.ZoomFactor ?? 1.0;
		var scaledHalfHandleSize = ( SelectionHandleSize / 2 ) / zoomFactor;

		context.Paint.StrokeWidth = SelectionBoxStrokeWidth;
		context.Paint.PathEffect = null;

		DrawSelectionHandle(rect.TopLeft);
		DrawSelectionHandle(rect.TopMiddle);
		DrawSelectionHandle(rect.TopRight);
		DrawSelectionHandle(rect.MiddleRight);
		DrawSelectionHandle(rect.BottomRight);
		DrawSelectionHandle(rect.BottomMiddle);
		DrawSelectionHandle(rect.BottomLeft);
		DrawSelectionHandle(rect.MiddleLeft);

		void DrawSelectionHandle(Point point)
		{
			var topLeft = new Point(point.X - scaledHalfHandleSize, point.Y - scaledHalfHandleSize);
			var bottomRight = new Point(point.X + scaledHalfHandleSize, point.Y + scaledHalfHandleSize);
			var handleRect = new Rect(topLeft, bottomRight);

			context.Paint.Style = SKPaintStyle.Fill;
			context.Paint.Color = White;
			context.Canvas.DrawRect(handleRect.ToSKRect(), context.Paint);
			context.Paint.Style = SKPaintStyle.Stroke;
			context.Paint.Color = Black;
			context.Canvas.DrawRect(handleRect.ToSKRect(), context.Paint);
		}
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

	private static bool TryGetRenderer(GUI__CDisplayObject displayObject, [NotNullWhen(true)] out IDisplayObjectRenderer? renderer)
	{
		var candidateType = displayObject.GetType();

		while (candidateType != null)
		{
			if (DisplayObjectRenderers.TryGetValue(candidateType, out renderer))
				return true;

			candidateType = candidateType.BaseType;
		}

		renderer = null;
		return false;
	}
}