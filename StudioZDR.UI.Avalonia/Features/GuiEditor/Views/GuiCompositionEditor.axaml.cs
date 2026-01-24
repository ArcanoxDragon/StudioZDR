using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData.Binding;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views;

internal partial class GuiCompositionEditor : ReactiveUserControl<GuiEditorViewModel>
{
	public static readonly FuncValueConverter<bool, string> IsVisibleIconConverter
		= new(isVisible => isVisible ? "fa-solid fa-eye" : "fa-solid fa-eye-slash");

	public static readonly DirectProperty<GuiCompositionEditor, bool> IsShiftPressedProperty
		= AvaloniaProperty.RegisterDirect<GuiCompositionEditor, bool>(nameof(IsShiftPressed), view => view.IsShiftPressed);

	internal static readonly StyledProperty<IList<GuiCompositionNodeViewModel>> SelectedNodesForPropertiesProperty
		= AvaloniaProperty.Register<GuiCompositionEditor, IList<GuiCompositionNodeViewModel>>(nameof(SelectedNodesForProperties));

	private readonly HashSet<string> expandedHierarchyNodes = [];

	private GuiCompositionNodeViewModel? previousHierarchy;
	private bool                         leftShiftPressed, rightShiftPressed;
	private string?                      hierarchyRootName;

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	public GuiCompositionEditor()
	{
		InitializeComponent();

		this.WhenActivated(disposables => {
			KeyDownEvent.AddClassHandler<TopLevel>(Global_OnKeyDown, handledEventsToo: true).DisposeWith(disposables);
			KeyUpEvent.AddClassHandler<TopLevel>(Global_OnKeyUp, handledEventsToo: true).DisposeWith(disposables);

			ViewModel?.WhenAnyValue(vm => vm.Composition!.HierarchyRootCollection)
				.Subscribe(_ => OnHierarchyReplaced())
				.DisposeWith(disposables);

			// In order for the data template to be invoked for any change in selection,
			// we need to *replace* the collection used for the template's binding,
			// instead of just relying on the collection itself being observable.
			ViewModel?.WhenAnyValue(vm => vm.SelectedNodes)
				.SelectMany(nodes => nodes.ToObservableChangeSet())
				.Subscribe(_ => SelectedNodesForProperties = ViewModel?.SelectedNodes.ToList() ?? []);

			OnHierarchyReplaced();
		});

		this.SetupDragDrop(CanDragTreeItem, CreateTreeItemDragData, OnTreeItemDragOver, OnTreeItemDrop, DragDropEffects.Move);
	}

	public bool IsShiftPressed
	{
		get;
		private set => SetAndRaise(IsShiftPressedProperty, ref field, value);
	}

	public IList<GuiCompositionNodeViewModel> SelectedNodesForProperties
	{
		get => GetValue(SelectedNodesForPropertiesProperty);
		private set => SetValue(SelectedNodesForPropertiesProperty, value);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		this.Canvas.Focus();
	}

	private void TreeView_OnPointerMoved(object? sender, PointerEventArgs e)
	{
		if (sender is not TreeView treeView)
			return;
		if (ViewModel is not { } viewModel)
			return;

		var point = e.GetPosition(treeView);
		var hoveredVisual = treeView.GetVisualAt(point);
		var hoveredTreeViewItem = hoveredVisual.FindAncestorOfType<TreeViewItem>();

		viewModel.HoveredNode = hoveredTreeViewItem?.DataContext as GuiCompositionNodeViewModel;
	}

	private void TreeView_OnPointerExited(object? sender, PointerEventArgs e)
	{
		if (ViewModel is not { } viewModel)
			return;

		viewModel.HoveredNode = null;
	}

	private void Global_OnKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.LeftShift)
			this.leftShiftPressed = true;
		else if (e.Key == Key.RightShift)
			this.rightShiftPressed = true;

		IsShiftPressed = this.leftShiftPressed || this.rightShiftPressed;
	}

	private void Global_OnKeyUp(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.LeftShift)
			this.leftShiftPressed = false;
		else if (e.Key == Key.RightShift)
			this.rightShiftPressed = false;

		IsShiftPressed = this.leftShiftPressed || this.rightShiftPressed;
	}

	private void ToggleVisible_OnClick(object? sender, RoutedEventArgs e)
	{
		if (ViewModel is not { } viewModel)
			return;
		if (sender is not Button button)
			return;

		if (viewModel.SelectedNodes.Contains(button.DataContext))
		{
			// The button's item was selected, so we affect all selected items
			foreach (var node in viewModel.SelectedNodes)
				node.ToggleVisible(IsShiftPressed);
		}
		else if (button.DataContext is GuiCompositionNodeViewModel node)
		{
			// The button's item was not selected, so we only affect it
			node.ToggleVisible(IsShiftPressed);
		}
	}

	private void ExpandAll_OnClick(object? sender, RoutedEventArgs e)
	{
		if (ViewModel?.Composition?.Hierarchy is not { } hierarchy)
			return;

		foreach (var node in hierarchy.EnumerateSelfAndChildren())
			node.IsExpanded = true;
	}

	private void CollapseAll_OnClick(object? sender, RoutedEventArgs e)
	{
		if (ViewModel?.Composition?.Hierarchy is not { } hierarchy)
			return;

		foreach (var node in hierarchy.EnumerateSelfAndChildren())
			node.IsExpanded = false;
	}

	private void OnHierarchyReplaced()
	{
		string? rootName = ViewModel?.Composition?.Hierarchy.Name;

		this.expandedHierarchyNodes.Clear();

		if (this.previousHierarchy != null)
		{
			foreach (var node in this.previousHierarchy.EnumerateSelfAndChildren())
			{
				if (node.IsExpanded)
					this.expandedHierarchyNodes.Add(node.FullPath);
			}
		}

		if (string.Equals(rootName, this.hierarchyRootName))
		{
			// Same hierarchy - try to restore our state
			RestoreExpandedNodes();
		}
		else
		{
			// Different hierarchy - clear the persisted state
			this.expandedHierarchyNodes.Clear();
		}

		this.hierarchyRootName = rootName;
		this.previousHierarchy = ViewModel?.Composition?.Hierarchy;
	}

	private void RestoreExpandedNodes()
	{
		if (ViewModel?.Composition?.Hierarchy is not { } hierarchy)
			return;

		foreach (var node in hierarchy.EnumerateSelfAndChildren())
			node.IsExpanded = this.expandedHierarchyNodes.Contains(node.FullPath);
	}

	#region Tree Item Drag/Drop

	private const double EdgeTolerance   = 20;
	private const double ScrollIncrement = 2;

	private static readonly TimeSpan ScrollInterval = TimeSpan.FromSeconds(1.0 / 30.0);

	private bool scrollingToTop;
	private bool scrollingToBottom;

	private bool CanDragTreeItem(Visual element)
	{
		var treeViewItem = element.FindAncestorOfType<TreeViewItem>(includeSelf: true);

		return treeViewItem != null && this.TreeView.TreeItemFromContainer(treeViewItem) is GuiCompositionNodeViewModel;
	}

	private IDataTransfer? CreateTreeItemDragData(Visual element)
	{
		var treeViewItem = element.FindAncestorOfType<TreeViewItem>(includeSelf: true);

		if (treeViewItem is null || this.TreeView.TreeItemFromContainer(treeViewItem) is not GuiCompositionNodeViewModel node)
			return null;

		return new GuiNodeDataTransfer(node);
	}

	private void OnTreeItemDragOver(object? sender, DragEventArgs e)
	{
		ValidateDragDrop(e, out _, out _); // No need for output here - just need to set drop effect

		var viewport = this.TreeViewScrollViewer.Viewport;
		var relative = e.GetPosition(this.TreeViewScrollViewer);

		if (relative.Y < EdgeTolerance)
		{
			// Start scrolling up
			this.scrollingToBottom = false;
			this.scrollingToTop = true;
			DispatcherTimer.Run(ScrollTowardsTop, ScrollInterval);
		}
		else if (relative.Y >= viewport.Height - EdgeTolerance)
		{
			// Start scrolling down
			this.scrollingToTop = false;
			this.scrollingToBottom = true;
			DispatcherTimer.Run(ScrollTowardsBottom, ScrollInterval);
		}
		else
		{
			this.scrollingToTop = this.scrollingToBottom = false;
		}
	}

	private bool ScrollTowardsTop()
	{
		var curOffset = this.TreeViewScrollViewer.Offset;

		this.TreeViewScrollViewer.Offset = curOffset.WithY(Math.Max(0, curOffset.Y - ScrollIncrement));

		return this.scrollingToTop;
	}

	private bool ScrollTowardsBottom()
	{
		var extent = this.TreeViewScrollViewer.Extent;
		var viewport = this.TreeViewScrollViewer.Viewport;
		var curOffset = this.TreeViewScrollViewer.Offset;

		this.TreeViewScrollViewer.Offset = curOffset.WithY(Math.Min(extent.Height - viewport.Height, curOffset.Y + ScrollIncrement));

		return this.scrollingToBottom;
	}

	private void OnTreeItemDrop(object? sender, DragEventArgs e)
	{
		this.scrollingToTop = this.scrollingToBottom = false;

		if (!ValidateDragDrop(e, out var sourceNode, out var targetNode))
			return;

		if (ViewModel is not { Composition: { } composition })
			return;
		if (sourceNode.DisplayObject is not { } sourceObj)
			return;
		if (targetNode.DisplayObject is not GUI__CDisplayObjectContainer targetContainer)
			return;

		if (sourceNode.Parent?.DisplayObject is GUI__CDisplayObjectContainer prevContainer)
		{
			// Remove source object from its old parent container
			prevContainer.Children.Remove(sourceObj);
		}

		// Add source object to target container
		targetContainer.Children.Add(sourceObj);

		// Stage undo operation
		ViewModel.StageUndoOperation();

		// Rebuild hierarchy
		composition.RebuildHierarchy();
	}

	private bool ValidateDragDrop(
		DragEventArgs e,
		[NotNullWhen(true)] out GuiCompositionNodeViewModel? sourceNode,
		[NotNullWhen(true)] out GuiCompositionNodeViewModel? targetNode)
	{
		// Set these first to allow early returns
		sourceNode = targetNode = null;
		e.DragEffects = DragDropEffects.None;

		if (e.DataTransfer is not GuiNodeDataTransfer dataTransfer ||
			e.Source is not Visual dropTarget ||
			dropTarget.FindAncestorOfType<TreeViewItem>(includeSelf: true) is not { } treeViewItem ||
			this.TreeView.TreeItemFromContainer(treeViewItem) is not GuiCompositionNodeViewModel dropTargetNode)
		{
			return false;
		}

		sourceNode = dataTransfer.Node;
		targetNode = dropTargetNode;

		// Ensure the target node is a CDisplayObjectContainer
		if (targetNode.DisplayObject is not GUI__CDisplayObjectContainer)
			return false;

		// Ensure the source node is not already a descendant of the target node
		var testNode = targetNode;

		while (testNode != null)
		{
			if (testNode == sourceNode)
				return false;

			testNode = testNode.Parent;
		}

		// If we reach this point, the drag/drop operation is allowed
		e.DragEffects = DragDropEffects.Move;
		return true;
	}

	private sealed class GuiNodeDataTransfer(GuiCompositionNodeViewModel node) : IDataTransfer
	{
		public GuiCompositionNodeViewModel Node { get; } = node;

		public IReadOnlyList<DataFormat>        Formats => [];
		public IReadOnlyList<IDataTransferItem> Items   { get; } = [new Item(node)];

		public void Dispose() { }

		private sealed class Item(GuiCompositionNodeViewModel itemNode) : IDataTransferItem
		{
			public object TryGetRaw(DataFormat format)
				=> itemNode;

			public IReadOnlyList<DataFormat> Formats => [];
		}
	}

	#endregion
}