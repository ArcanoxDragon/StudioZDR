using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;
using StudioZDR.UI.Avalonia.Rendering;
using StudioZDR.UI.Avalonia.Rendering.DreadGui;
using Vector = Avalonia.Vector;

#if DEBUG
// For hot reload
using Avalonia.Threading;
using StudioZDR.UI.Avalonia.Utility;
#endif

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views;

internal partial class GuiCompositionCanvas : ContentControl
{
	public const  double MinimumZoomLevel    = -1;
	public const  double MaximumZoomLevel    = 1;
	private const double MinimumDragDistance = 5;

	public static readonly StyledProperty<DreadGuiCompositionViewModel?> CompositionProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, DreadGuiCompositionViewModel?>(nameof(Composition));

	public static readonly StyledProperty<GuiEditorViewModel?> EditorProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, GuiEditorViewModel?>(nameof(Editor));

	private readonly SpriteSheetManager spriteSheetManager;

	private CompositeDisposable?              disposables;
	private DreadGuiCompositionDrawOperation? drawOperation;

	// Panning
	private bool    panning;
	private Point   panStart;
	private Vector2 initialPanOffset;

	// Dragging
	private readonly List<DraggingObject> draggingObjects = [];
	private          bool                 dragging;
	private          bool                 dragLatched;
	private          Point                dragStart;

	public GuiCompositionCanvas()
	{
		Focusable = true;

		this.spriteSheetManager = App.Container.Resolve<SpriteSheetManager>();

		InitializeComponent();
	}

	public DreadGuiCompositionViewModel? Composition
	{
		get => GetValue(CompositionProperty);
		set => SetValue(CompositionProperty, value);
	}

	public GuiEditorViewModel? Editor
	{
		get => GetValue(EditorProperty);
		set => SetValue(EditorProperty, value);
	}

	private Matrix TransformMatrix        { get; set; } = Matrix.Identity;
	private Matrix InverseTransformMatrix { get; set; } = Matrix.Identity;
	private Point  LastCursorPosition     { get; set; }

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		this.drawOperation?.DisposeFinal();
		this.drawOperation = new DreadGuiCompositionDrawOperation(this.spriteSheetManager);

		AttachSubscriptions();

#if DEBUG
		HotReloadHelper.MetadataUpdated += HandleMetadataReloaded;
#endif
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		this.drawOperation?.DisposeFinal();
		this.drawOperation = null;

		DetachSubscriptions();

#if DEBUG
		HotReloadHelper.MetadataUpdated -= HandleMetadataReloaded;
#endif
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		if (Editor is not { } editor)
			return;
		if (this.panning || this.dragging)
			// Don't allow starting one if already doing another
			return;

		var point = e.GetCurrentPoint(this);

		if (point.Properties.IsBarrelButtonPressed || point.Properties.IsRightButtonPressed)
		{
			// Begin panning
			e.PreventGestureRecognition();
			point.Pointer.Capture(this);
			this.panStart = point.Position;
			this.initialPanOffset = editor.PanOffset;
			this.panning = true;
		}
		else if (point.Properties.IsLeftButtonPressed) // TODO: Verify this is true for primary pen/touch input
		{
			// Select or transform objects

			e.PreventGestureRecognition();
			point.Pointer.Capture(this);

			var hoveredNodes = GetHoveredNodes().ToList();
			var anySelectedUnderPointer = editor.SelectedNodes.Intersect(hoveredNodes).Any();

			if (anySelectedUnderPointer)
			{
				// Immediately start dragging
				StartDragging();
			}
			else if (editor.IsMouseSelectionEnabled)
			{
				// We only care about the "inner-most" (i.e. "front-most", to the user) child that is under the pointer
				var innermostHoveredNode = hoveredNodes.LastOrDefault();

				if (innermostHoveredNode is null)
				{
					// De-select everything
					editor.SelectedNodes.Clear();
					return;
				}

				if (( e.KeyModifiers & KeyModifiers.Control ) == KeyModifiers.Control)
				{
					// Toggle the node as being selected
					editor.ToggleSelected(innermostHoveredNode);
				}
				else if (!editor.SelectedNodes.Contains(innermostHoveredNode))
				{
					// Make this node the only selected node (and start dragging)
					editor.SelectedNodes.Clear();
					editor.SelectedNodes.Add(innermostHoveredNode);
					StartDragging();
				}
			}
		}

		void StartDragging()
		{
			this.dragStart = point.Position;
			this.draggingObjects.AddRange(editor.GetTopmostSelectedNodes().Select(node => new DraggingObject(node, GetInitialObjectPosition(node))));
			this.dragLatched = false;
			this.dragging = true;
		}

		Point GetInitialObjectPosition(GuiCompositionNodeViewModel node)
		{
			GetNodeBounds(node, out var origin);
			return origin;
		}
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		var newPosition = e.GetPosition(this);

		LastCursorPosition = newPosition;
		InvalidateVisual();

		if (Editor is not { } editor)
			return;

		if (this.panning)
		{
			var delta = newPosition - this.panStart;

			editor.PanOffset = this.initialPanOffset + new Vector2((float) delta.X, (float) delta.Y);
			Cursor = new Cursor(StandardCursorType.SizeAll);
		}
		else if (this.dragging)
		{
			var delta = ( newPosition - this.dragStart ) / editor.ZoomFactor;

			if (!this.dragLatched && ( (Vector) delta ).Length > MinimumDragDistance)
				this.dragLatched = true;

			if (this.dragLatched)
			{
				Rect screenBounds = GetScreenBounds();

				foreach (var (node, initialPosition) in this.draggingObjects)
				{
					if (node.DisplayObject is not { } displayObject)
						continue;

					Rect parentBounds = node.Parent is null ? screenBounds : GetNodeBounds(node.Parent, out _);
					Point newObjectPosition = initialPosition + delta;

					displayObject.MoveOriginTo(screenBounds, parentBounds, newObjectPosition);
					node.NotifyOfDisplayObjectChange();
				}
			}
		}
		else if (editor.IsMouseSelectionEnabled)
		{
			var hoveredNodes = GetHoveredNodes().ToList();
			var anySelectedUnderPointer = editor.SelectedNodes.Intersect(hoveredNodes).Any();

			if (anySelectedUnderPointer)
			{
				// If the pointer is over any selected nodes, don't show the selection "ghost"
				editor.HoveredNode = null;
				Cursor = new Cursor(StandardCursorType.SizeAll);
			}
			else
			{
				// We want the "innermost" node to appear hovered, but not any of its parents
				editor.HoveredNode = hoveredNodes.LastOrDefault();
				Cursor = Cursor.Default;
			}
		}
		else
		{
			// Reset the cursor
			Cursor = Cursor.Default;
		}
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		EndPointerActions();
		base.OnPointerReleased(e);
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
	{
		EndPointerActions();
		base.OnPointerCaptureLost(e);
	}

	private void EndPointerActions()
	{
		if (this.dragLatched && Editor is { } editor)
			editor.StageUndoOperation();

		this.panning = false;
		this.dragging = false;
		this.dragLatched = false;
		this.draggingObjects.Clear();
	}

	private IEnumerable<GuiCompositionNodeViewModel> GetHoveredNodes()
	{
		if (Composition is not { Hierarchy: { } hierarchy })
			yield break;

		Rect screenBounds = GetScreenBounds();
		Point transformedCursorPosition = LastCursorPosition.Transform(InverseTransformMatrix);

		foreach (var found in Visit(hierarchy, screenBounds))
			yield return found;

		IEnumerable<GuiCompositionNodeViewModel> Visit(GuiCompositionNodeViewModel node, Rect parentBounds)
		{
			if (node is not { IsVisible: true, DisplayObject: { } displayObject })
				yield break;

			var objBounds = displayObject.GetDisplayObjectRect(screenBounds, parentBounds);

			if (objBounds.Contains(transformedCursorPosition))
				yield return node;

			foreach (var child in node.Children)
			foreach (var found in Visit(child, objBounds))
				yield return found;
		}
	}

	private Rect GetScreenBounds()
		=> this.drawOperation?.RenderBounds ?? new Rect(0, 0, Bounds.Width, Bounds.Height);

	private Rect GetNodeBounds(GuiCompositionNodeViewModel node, out Point origin)
	{
		if (node.DisplayObject is not { } displayObject)
		{
			origin = default;
			return new Rect();
		}

		Rect screenBounds = GetScreenBounds();
		Rect parentBounds = node.Parent is null ? screenBounds : GetNodeBounds(node.Parent, out _);

		return displayObject.GetDisplayObjectRect(screenBounds, parentBounds, out origin);
	}

	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
		base.OnPointerWheelChanged(e);

		const double Multiplier = 0.05;

		if (Editor is not { } editor)
			return;

		editor.ZoomLevel = Math.Clamp(editor.ZoomLevel + ( e.Delta.Y * Multiplier ), MinimumZoomLevel, MaximumZoomLevel);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == CompositionProperty)
		{
			if (change.OldValue is DreadGuiCompositionViewModel oldComposition)
				oldComposition.Disposing -= OnCompositionDisposing;
			if (change.NewValue is DreadGuiCompositionViewModel newComposition)
				newComposition.Disposing += OnCompositionDisposing;

			ReleaseCurrentlyRenderingComposition();
			AttachSubscriptions();
			InvalidateVisual();
			CalculateMatrix();
		}

		if (change.Property == EditorProperty)
		{
			AttachSubscriptions();
			InvalidateVisual();
			CalculateMatrix();
		}
	}

	private void OnCompositionDisposing(object? sender, EventArgs e)
		=> ReleaseCurrentlyRenderingComposition();

	private void ReleaseCurrentlyRenderingComposition()
	{
		if (this.drawOperation is null)
			return;

		// To ensure we don't try to render a disposing composition,
		// we need to null out the draw operation's reference to any
		// composition, and then wait for the draw operation to finish
		// rendering (if it currently is).
		this.drawOperation.Composition = null;
		this.drawOperation.WaitForRendering();
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var backgroundRect = new Rect(default, Bounds.Size);

		context.FillRectangle(Brushes.Black, backgroundRect);

		if (this.drawOperation is null)
			return;

		this.drawOperation.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
		this.drawOperation.Composition = Composition;
		this.drawOperation.Editor = Editor;
		context.Custom(this.drawOperation);
	}

	private void CalculateMatrix()
	{
		if (Editor is not { } editor || this.drawOperation is not { } drawOperation)
		{
			TransformMatrix = InverseTransformMatrix = Matrix.Identity;
			return;
		}

		var zoomFactor = editor.ZoomFactor;
		var centerX = drawOperation.RenderBounds.Width / 2;
		var centerY = drawOperation.RenderBounds.Height / 2;
		var centerPoint = new Vector(centerX, centerY);

		TransformMatrix = Matrix.CreateTranslation(-centerPoint) *
						  Matrix.CreateScale(zoomFactor, zoomFactor) *
						  Matrix.CreateTranslation(centerPoint) *
						  Matrix.CreateTranslation(editor.PanOffset);
		InverseTransformMatrix = TransformMatrix.TryInvert(out var inverted) ? inverted : Matrix.Identity;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	private void AttachSubscriptions()
	{
		this.disposables?.Dispose();
		this.disposables = [];

		this.spriteSheetManager.SpriteLoaded
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => InvalidateVisual())
			.DisposeWith(this.disposables);

		Editor?.WhenAnyValue(m => m.ZoomFactor, m => m.PanOffset, m => m.HoveredNode, m => m.SelectedNodes)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => InvalidateVisual())
			.DisposeWith(this.disposables);

		Editor?.WhenAnyValue(m => m.ZoomFactor, m => m.PanOffset)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => CalculateMatrix())
			.DisposeWith(this.disposables);

		Composition?.RenderInvalidated
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => InvalidateVisual())
			.DisposeWith(this.disposables);
	}

	private void DetachSubscriptions()
	{
		this.disposables?.Dispose();
		this.disposables = null;
	}

#if DEBUG
	private void HandleMetadataReloaded(Type[]? reloadedTypes)
	{
		if (reloadedTypes?.Contains(typeof(DreadGuiCompositionDrawOperation)) is true)
			Dispatcher.UIThread.Invoke(InvalidateVisual);
	}
#endif

	private sealed record DraggingObject(GuiCompositionNodeViewModel Node, Point InitialPosition);
}