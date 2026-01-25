using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Humanizer;
using ReactiveUI;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;
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
	public const  double MinimumZoomLevel             = -1;
	public const  double MaximumZoomLevel             = 1;
	private const double MinimumDragDistance          = 5;
	private const double MinimumDragDistanceSquared   = MinimumDragDistance * MinimumDragDistance;
	private const double ResizeHandleTolerance        = 5;
	private const double ResizeHandleToleranceSquared = ResizeHandleTolerance * ResizeHandleTolerance;

	private static readonly TimeSpan DoubleTapTime = 350.Milliseconds();

	#region Properties

	public static readonly StyledProperty<DreadGuiCompositionViewModel?> CompositionProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, DreadGuiCompositionViewModel?>(nameof(Composition));

	public static readonly StyledProperty<GuiEditorViewModel?> EditorProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, GuiEditorViewModel?>(nameof(Editor));

	#endregion

	private readonly SpriteSheetManager spriteSheetManager;

	private CompositeDisposable?              disposables;
	private DreadGuiCompositionDrawOperation? drawOperation;

	// Misc. State
	private DateTime lastPressedEscape;

	// Panning
	private bool    panning;
	private Point   panStart;
	private Vector2 initialPanOffset;

	// Dragging
	private readonly List<DraggingObject> draggingObjects = [];
	private          bool                 dragging;
	private          bool                 dragLatched;
	private          Point                dragStart;
	private          bool                 dragIsResize;
	private          ResizeType           resizeType;

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

	#region Pointer Events

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		if (Editor is not { } editor)
			return;
		if (this.panning || this.dragging)
			// Don't allow starting one if already doing another
			return;

		var ctrlPressed = ( e.KeyModifiers & KeyModifiers.Control ) == KeyModifiers.Control;
		var screenBounds = GetScreenBounds().ToRectangleF();
		var point = e.GetCurrentPoint(this);
		var transformedPosition = point.Position.Transform(InverseTransformMatrix);

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
			var selectionBounds = editor.SelectedNodes.GetOverallBounds(screenBounds);
			var considerSelection = editor.SelectedNodes.Count > 0 && !ctrlPressed;
			var pointerInsideSelection = considerSelection && selectionBounds.Contains(transformedPosition.ToPointF());

			if (considerSelection && TryGetResizeType(selectionBounds.ToAvalonia(), transformedPosition, out var resizeType))
			{
				// Clicked on a resize handle - immediately start resizing
				StartDragging(resizeType);
			}
			else if (pointerInsideSelection)
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

				if (ctrlPressed)
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

		void StartDragging(ResizeType? resizeType = null)
		{
			this.dragStart = transformedPosition;
			this.draggingObjects.AddRange(editor.GetTopmostSelectedNodes().Select(node => new DraggingObject(node, GetInitialObjectPosition(node))));
			this.dragLatched = resizeType.HasValue; // Resize drags start immediately without needing a large enough "latching" movement
			this.dragging = true;
			this.dragIsResize = resizeType.HasValue;
			this.resizeType = resizeType ?? default;
		}

		Point GetInitialObjectPosition(GuiCompositionNodeViewModel node)
			=> node.GetTransform(screenBounds).Origin.ToAvalonia();
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		var ctrlPressed = ( e.KeyModifiers & KeyModifiers.Control ) == KeyModifiers.Control;
		var newPosition = e.GetPosition(this);
		var transformedPosition = newPosition.Transform(InverseTransformMatrix);

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
			var delta = transformedPosition - this.dragStart;
			var dragLengthSquared = ( (Vector) delta ).SquaredLength;

			if (!this.dragLatched && dragLengthSquared > MinimumDragDistanceSquared)
				this.dragLatched = true;

			if (this.dragLatched)
				MoveDraggedObjects(transformedPosition, delta);
		}
		else
		{
			var screenBounds = GetScreenBounds().ToRectangleF();
			var selectionBounds = editor.SelectedNodes.GetOverallBounds(screenBounds);
			var considerSelection = editor.SelectedNodes.Count > 0 && !ctrlPressed;
			var pointerInsideSelection = considerSelection && selectionBounds.Contains(transformedPosition.ToPointF());
			GuiCompositionNodeViewModel? hoveredNode = null;

			if (considerSelection && TryGetResizeType(selectionBounds.ToAvalonia(), transformedPosition, out var resizeType))
			{
				// Cursor is near a resize handle
				Cursor = new Cursor(resizeType.Cursor);
			}
			else if (pointerInsideSelection)
			{
				// If the pointer is over any selected nodes, don't show the selection "ghost"
				Cursor = new Cursor(StandardCursorType.SizeAll);
			}
			else if (editor.IsMouseSelectionEnabled)
			{
				var hoveredNodes = GetHoveredNodes().ToList();

				// We want the "innermost" node to appear hovered, but not any of its parents
				hoveredNode = hoveredNodes.LastOrDefault();
				Cursor = Cursor.Default;
			}
			else
			{
				// Reset the cursor and hovered node
				Cursor = Cursor.Default;
			}

			editor.HoveredNode = hoveredNode;
		}
	}

	private void MoveDraggedObjects(Point cursorPosition, Point delta)
	{
		var (resizeOrigin, _, doResizeX, doResizeY) = this.resizeType;
		var screenBounds = GetScreenBounds().ToRectangleF();
		var fullResizeDistanceX = Math.Abs(this.dragStart.X - resizeOrigin.X);
		var fullResizeDistanceY = Math.Abs(this.dragStart.Y - resizeOrigin.Y);
		var resizeDirectionX = Math.Sign(this.dragStart.X - resizeOrigin.X);
		var resizeDirectionY = Math.Sign(this.dragStart.Y - resizeOrigin.Y);
		var resizeScaleX = ( cursorPosition.X - resizeOrigin.X ) / fullResizeDistanceX * resizeDirectionX;
		var resizeScaleY = ( cursorPosition.Y - resizeOrigin.Y ) / fullResizeDistanceY * resizeDirectionY;

		foreach (var draggingObject in this.draggingObjects)
		{
			var (node, initialPosition) = draggingObject;

			if (node.DisplayObject is not { } displayObject)
				continue;

			var parentBounds = node.Parent?.GetTransform(screenBounds) ?? GuiTransform.CreateDefault(screenBounds);
			Point newObjectPosition;

			if (this.dragIsResize)
			{
				// Objects move by an amount proportional to their distance from the drag origin
				var objectToResizeOrigin = initialPosition - resizeOrigin;
				var proportionalResizeX = Math.Abs(fullResizeDistanceX) > 1e-20
					? Math.Abs(objectToResizeOrigin.X / fullResizeDistanceX)
					: 0;
				var proportionalResizeY = Math.Abs(fullResizeDistanceY) > 1e-20
					? Math.Abs(objectToResizeOrigin.Y / fullResizeDistanceY)
					: 0;
				var deltaScaleX = doResizeX ? proportionalResizeX : 0;
				var deltaScaleY = doResizeY ? proportionalResizeY : 0;
				var objDelta = new Point(delta.X * deltaScaleX, delta.Y * deltaScaleY);

				newObjectPosition = initialPosition + objDelta;

				if (doResizeX)
					displayObject.SizeX = draggingObject.InitialSizeX * (float) resizeScaleX;
				if (doResizeY)
					displayObject.SizeY = draggingObject.InitialSizeY * (float) resizeScaleY;
			}
			else
			{
				// All objects move by the same amount
				newObjectPosition = initialPosition + delta;
			}

			displayObject.MoveOriginTo(parentBounds, newObjectPosition.ToPointF());
			node.NotifyOfDisplayObjectChange();
		}
	}

	private static bool TryGetResizeType(Rect bounds, Point point, out ResizeType resizeType)
	{
		var origin = default(Point);
		var cursorResult = default(StandardCursorType);
		var resizeXResult = false;
		var resizeYResult = false;
		var success = Try(bounds.TopLeft, bounds.BottomRight, StandardCursorType.TopLeftCorner, resizeX: true, resizeY: true) ||
					  Try(bounds.TopMiddle, bounds.BottomMiddle, StandardCursorType.TopSide, resizeX: false, resizeY: true) ||
					  Try(bounds.TopRight, bounds.BottomLeft, StandardCursorType.TopRightCorner, resizeX: true, resizeY: true) ||
					  Try(bounds.MiddleRight, bounds.MiddleLeft, StandardCursorType.RightSide, resizeX: true, resizeY: false) ||
					  Try(bounds.BottomRight, bounds.TopLeft, StandardCursorType.BottomRightCorner, resizeX: true, resizeY: true) ||
					  Try(bounds.BottomMiddle, bounds.TopMiddle, StandardCursorType.BottomSide, resizeX: false, resizeY: true) ||
					  Try(bounds.BottomLeft, bounds.TopRight, StandardCursorType.BottomLeftCorner, resizeX: true, resizeY: true) ||
					  Try(bounds.MiddleLeft, bounds.MiddleRight, StandardCursorType.LeftSide, resizeX: true, resizeY: false);

		resizeType = new ResizeType(origin, cursorResult, resizeXResult, resizeYResult);
		return success;

		bool Try(Point testPoint, Point originCandidate, StandardCursorType cursor, bool resizeX, bool resizeY)
		{
			var squaredDistance = ( (Vector) ( point - testPoint ) ).SquaredLength;
			var isSuccess = squaredDistance < ResizeHandleToleranceSquared;

			if (isSuccess)
			{
				origin = originCandidate;
				cursorResult = cursor;
				resizeXResult = resizeX;
				resizeYResult = resizeY;
			}

			return isSuccess;
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
		this.dragIsResize = false;
		this.dragLatched = false;
		this.draggingObjects.Clear();
	}

	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
		base.OnPointerWheelChanged(e);

		const double Multiplier = 0.05;

		if (Editor is not { } editor)
			return;

		editor.ZoomLevel = Math.Clamp(editor.ZoomLevel + ( e.Delta.Y * Multiplier ), MinimumZoomLevel, MaximumZoomLevel);
	}

	#endregion

	#region Keyboard Events

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.Key == Key.Escape)
		{
			DateTime pressTime = DateTime.Now;

			if (pressTime - this.lastPressedEscape < DoubleTapTime)
			{
				// Deselect all objects when double-tapping escape
				Editor?.SelectedNodes.Clear();
			}

			this.lastPressedEscape = pressTime;
		}
	}

	#endregion

	#region Helpers

	private IEnumerable<GuiCompositionNodeViewModel> GetHoveredNodes()
	{
		if (Composition is not { Hierarchy: { } hierarchy })
			yield break;

		var screenBounds = GetScreenBounds().ToRectangleF();
		var rootBounds = GuiTransform.CreateDefault(screenBounds);
		var transformedCursorPosition = LastCursorPosition.Transform(InverseTransformMatrix);

		foreach (var found in Visit(hierarchy, rootBounds))
			yield return found;

		IEnumerable<GuiCompositionNodeViewModel> Visit(GuiCompositionNodeViewModel node, GuiTransform parentTransform)
		{
			if (node is not { IsVisible: true, DisplayObject: { } displayObject })
				yield break;

			var objBounds = displayObject.GetTransform(parentTransform);

			if (objBounds.AxisAlignedBoundingBox.Contains(transformedCursorPosition.ToPointF()))
				yield return node;

			foreach (var child in node.Children)
			foreach (var found in Visit(child, objBounds))
				yield return found;
		}
	}

	private Rect GetScreenBounds()
		=> this.drawOperation?.RenderBounds ?? new Rect(0, 0, Bounds.Width, Bounds.Height);

	#endregion

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

	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
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

	private sealed record DraggingObject(GuiCompositionNodeViewModel Node, Point InitialPosition)
	{
		public float InitialSizeX { get; } = Node.DisplayObject?.SizeX ?? 1f;
		public float InitialSizeY { get; } = Node.DisplayObject?.SizeY ?? 1f;
	}

	private readonly record struct ResizeType(Point Origin, StandardCursorType Cursor, bool X, bool Y);
}