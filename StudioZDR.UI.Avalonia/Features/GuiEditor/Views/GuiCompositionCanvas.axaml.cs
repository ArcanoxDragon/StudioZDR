using System.Diagnostics;
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
	public const double MinimumZoomLevel = -1;
	public const double MaximumZoomLevel = 1;

	public static readonly StyledProperty<DreadGuiCompositionViewModel?> ViewModelProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, DreadGuiCompositionViewModel?>(nameof(ViewModel));

	private readonly SpriteSheetManager spriteSheetManager;

	private CompositeDisposable?              disposables;
	private DreadGuiCompositionDrawOperation? drawOperation;

	private bool    panning;
	private Point   panStart;
	private Vector2 initialPanOffset;

	public GuiCompositionCanvas()
	{
		this.spriteSheetManager = App.Container.Resolve<SpriteSheetManager>();

		InitializeComponent();
	}

	public DreadGuiCompositionViewModel? ViewModel
	{
		get => GetValue(ViewModelProperty);
		set => SetValue(ViewModelProperty, value);
	}

	private Matrix TransformMatrix        { get; set; } = Matrix.Identity;
	private Matrix InverseTransformMatrix { get; set; } = Matrix.Identity;
	private Point  LastCursorPosition     { get; set; }

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		this.drawOperation?.Dispose();
		this.drawOperation = new DreadGuiCompositionDrawOperation(this.spriteSheetManager);

		AttachSubscriptions();

#if DEBUG
		HotReloadHelper.MetadataUpdated += HandleMetadataReloaded;
#endif
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		this.drawOperation?.Dispose();
		this.drawOperation = null;

		DetachSubscriptions();

#if DEBUG
		HotReloadHelper.MetadataUpdated -= HandleMetadataReloaded;
#endif
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		if (ViewModel is not { } viewModel)
			return;

		var point = e.GetCurrentPoint(this);

		if (point.Properties.IsBarrelButtonPressed || point.Properties.IsRightButtonPressed)
		{
			// Begin panning
			e.PreventGestureRecognition();
			point.Pointer.Capture(this);
			this.panStart = point.Position;
			this.initialPanOffset = viewModel.PanOffset;
			this.panning = true;
		}
		else if (point.Properties.IsLeftButtonPressed) // TODO: Verify this is true for primary pen/touch input
		{
			// Select or transform objects

			e.PreventGestureRecognition();
			point.Pointer.Capture(this);

			if (viewModel.IsMouseSelectionEnabled)
			{
				var hoveredNode = GetHoveredNode();

				if (hoveredNode is null)
				{
					// De-select everything
					viewModel.SelectedNodes.Clear();
					return;
				}

				if (( e.KeyModifiers & KeyModifiers.Control ) == KeyModifiers.Control)
				{
					// Toggle the node as being selected
					viewModel.ToggleSelected(hoveredNode);
				}
				else if (!viewModel.SelectedNodes.Contains(hoveredNode))
				{
					// Make this node the only selected node
					viewModel.SelectedNodes.Clear();
					viewModel.SelectedNodes.Add(hoveredNode);
				}
			}
		}
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		base.OnPointerReleased(e);

		this.panning = false;
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
	{
		base.OnPointerCaptureLost(e);

		this.panning = false;
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		var newPosition = e.GetPosition(this);

		LastCursorPosition = newPosition;
		InvalidateVisual();

		if (ViewModel is not { } viewModel)
			return;

		if (this.panning)
		{
			var delta = newPosition - this.panStart;

			viewModel.PanOffset = this.initialPanOffset + new Vector2((float) delta.X, (float) delta.Y);
		}
		else if (viewModel.IsMouseSelectionEnabled)
		{
			viewModel.HoveredNode = GetHoveredNode();
		}
	}

	private GuiCompositionNodeViewModel? GetHoveredNode()
	{
		if (ViewModel is not { Hierarchy: { } hierarchy })
			return null;
		if (this.drawOperation is not { } drawOperation)
			return null;

		Rect screenBounds = drawOperation.RenderBounds;
		Point transformedCursorPosition = LastCursorPosition.Transform(InverseTransformMatrix);
		GuiCompositionNodeViewModel? hovered = null;

		Visit(hierarchy, screenBounds);

		return hovered;

		void Visit(GuiCompositionNodeViewModel node, Rect parentBounds)
		{
			if (node.DisplayObject is not { } displayObject)
				return;

			var objBounds = displayObject.GetDisplayObjectRect(screenBounds, parentBounds);

			if (node.IsVisible && objBounds.Contains(transformedCursorPosition))
				hovered = node;

			foreach (var child in node.Children)
				Visit(child, objBounds);
		}
	}

	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
		base.OnPointerWheelChanged(e);

		const double Multiplier = 0.05;

		if (ViewModel is not { } viewModel)
			return;

		viewModel.ZoomLevel = Math.Clamp(viewModel.ZoomLevel + e.Delta.Y * Multiplier, MinimumZoomLevel, MaximumZoomLevel);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == ViewModelProperty)
		{
			AttachSubscriptions();
			InvalidateVisual();
			CalculateMatrix();
		}
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var backgroundRect = new Rect(default, Bounds.Size);

		context.FillRectangle(Brushes.Black, backgroundRect);

		if (this.drawOperation is null)
			return;

		this.drawOperation.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
		this.drawOperation.ViewModel = ViewModel;
		context.Custom(this.drawOperation);
	}

	private void CalculateMatrix()
	{
		if (ViewModel is not { } viewModel || this.drawOperation is not { } drawOperation)
		{
			TransformMatrix = InverseTransformMatrix = Matrix.Identity;
			return;
		}

		var zoomFactor = viewModel.ZoomFactor;
		var centerX = drawOperation.RenderBounds.Width / 2;
		var centerY = drawOperation.RenderBounds.Height / 2;
		var centerPoint = new Vector(centerX, centerY);

		TransformMatrix = Matrix.CreateTranslation(-centerPoint) *
						  Matrix.CreateScale(zoomFactor, zoomFactor) *
						  Matrix.CreateTranslation(centerPoint) *
						  Matrix.CreateTranslation(viewModel.PanOffset);
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

		ViewModel?.WhenAnyValue(m => m.ZoomFactor, m => m.PanOffset, m => m.HoveredNode, m => m.SelectedNodes)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => InvalidateVisual())
			.DisposeWith(this.disposables);

		ViewModel?.WhenAnyValue(m => m.ZoomFactor, m => m.PanOffset)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => CalculateMatrix())
			.DisposeWith(this.disposables);

		ViewModel?.RenderInvalidated
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(_ => {
				Debug.WriteLine("Render invalidated");
				InvalidateVisual();
			})
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
}