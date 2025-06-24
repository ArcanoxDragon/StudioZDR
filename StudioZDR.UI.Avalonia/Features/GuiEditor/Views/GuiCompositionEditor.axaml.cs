using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DynamicData.Binding;
using ReactiveUI;
using StudioZDR.App.Features.GuiEditor.ViewModels;

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

	private bool    leftShiftPressed, rightShiftPressed;
	private string? hierarchyRootName;

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	public GuiCompositionEditor()
	{
		InitializeComponent();

		KeyDownEvent.AddClassHandler<TopLevel>(Global_OnKeyDown, handledEventsToo: true);
		KeyUpEvent.AddClassHandler<TopLevel>(Global_OnKeyUp, handledEventsToo: true);

		this.WhenActivated(disposables => {
			ViewModel?.WhenAnyValue(vm => vm.Composition!.Hierarchy)
				.Subscribe(_ => OnHierarchyReplaced())
				.DisposeWith(disposables);

			// In order for the data template to be invoked for any change in selection,
			// we need to *replace* the collection used for the template's binding,
			// instead of just relying on the collection itself being observable.
			ViewModel?.WhenAnyValue(vm => vm.SelectedNodes)
				.SelectMany(nodes => nodes.ToObservableChangeSet())
				.Subscribe(_ => SelectedNodesForProperties = ViewModel?.SelectedNodes.ToList() ?? []);
		});
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
		foreach (var item in this.TreeView.GetVisualDescendants().OfType<TreeViewItem>())
			this.TreeView.ExpandSubTree(item);
	}

	private void CollapseAll_OnClick(object? sender, RoutedEventArgs e)
	{
		foreach (var item in this.TreeView.GetVisualDescendants().OfType<TreeViewItem>())
			this.TreeView.CollapseSubTree(item);
	}

	private void OnHierarchyReplaced()
	{
		string? rootName = ViewModel?.Composition?.Hierarchy.Name;

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
	}

	private void RestoreExpandedNodes()
	{
		foreach (var treeContainer in this.TreeView.GetRealizedTreeContainers().OfType<TreeViewItem>())
		{
			if (this.TreeView.TreeItemFromContainer(treeContainer) is not GuiCompositionNodeViewModel node)
				continue;

			treeContainer.IsExpanded = this.expandedHierarchyNodes.Contains(node.FullPath);
		}
	}

	private void TreeView_OnItemExpanded(object? sender, RoutedEventArgs e)
	{
		if (e.Source is not TreeViewItem item)
			return;
		if (this.TreeView.TreeItemFromContainer(item) is not GuiCompositionNodeViewModel node)
			return;

		this.expandedHierarchyNodes.Add(node.FullPath);
	}

	private void TreeView_OnItemCollapsed(object? sender, RoutedEventArgs e)
	{
		if (e.Source is not TreeViewItem item)
			return;
		if (this.TreeView.TreeItemFromContainer(item) is not GuiCompositionNodeViewModel node)
			return;

		this.expandedHierarchyNodes.Remove(node.FullPath);
	}
}