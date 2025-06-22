using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using StudioZDR.App.Features.GuiEditor.ViewModels;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views;

internal partial class GuiCompositionEditor : ReactiveUserControl<GuiEditorViewModel>
{
	public static readonly FuncValueConverter<bool, string> IsVisibleIconConverter
		= new(isVisible => isVisible ? "fa-solid fa-eye" : "fa-solid fa-eye-slash");

	public static readonly DirectProperty<GuiCompositionEditor, bool> IsShiftPressedProperty
		= AvaloniaProperty.RegisterDirect<GuiCompositionEditor, bool>(nameof(IsShiftPressed), view => view.IsShiftPressed);

	private bool leftShiftPressed, rightShiftPressed;

	public GuiCompositionEditor()
	{
		InitializeComponent();

		KeyDownEvent.AddClassHandler<TopLevel>(Global_OnKeyDown, handledEventsToo: true);
		KeyUpEvent.AddClassHandler<TopLevel>(Global_OnKeyUp, handledEventsToo: true);
	}

	public bool IsShiftPressed
	{
		get;
		set => SetAndRaise(IsShiftPressedProperty, ref field, value);
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
		if (this.TreeView.FindDescendantOfType<TreeViewItem>() is { } rootItem)
			this.TreeView.ExpandSubTree(rootItem);
	}

	private void CollapseAll_OnClick(object? sender, RoutedEventArgs e)
	{
		if (this.TreeView.FindDescendantOfType<TreeViewItem>() is { } rootItem)
			this.TreeView.CollapseSubTree(rootItem);
	}
}