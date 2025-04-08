using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.VisualTree;
using Material.Icons;
using StudioZDR.App.Features.GuiEditor.ViewModels;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views;

public partial class GuiEditorView : ReactiveUserControl<GuiEditorViewModel>
{
	public static readonly FuncValueConverter<bool, MaterialIconKind> IsVisibleIconConverter
		= new(isVisible => isVisible ? MaterialIconKind.Eye : MaterialIconKind.EyeClosed);

	public static readonly DirectProperty<GuiEditorView, bool> IsShiftPressedProperty
		= AvaloniaProperty.RegisterDirect<GuiEditorView, bool>(nameof(IsShiftPressed), view => view.IsShiftPressed);

	private bool leftShiftPressed, rightShiftPressed;

	public GuiEditorView()
	{
		InitializeComponent();

		InputElement.KeyDownEvent.AddClassHandler<TopLevel>(Global_OnKeyDown, handledEventsToo: true);
		InputElement.KeyUpEvent.AddClassHandler<TopLevel>(Global_OnKeyUp, handledEventsToo: true);
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
		if (ViewModel?.GuiCompositionViewModel is not { } viewModel)
			return;

		var point = e.GetPosition(treeView);
		var hoveredVisual = treeView.GetVisualAt(point);
		var hoveredTreeViewItem = hoveredVisual.FindAncestorOfType<TreeViewItem>();

		viewModel.HoveredNode = hoveredTreeViewItem?.DataContext as GuiCompositionNodeViewModel;
	}

	private void TreeView_OnPointerExited(object? sender, PointerEventArgs e)
	{
		if (ViewModel?.GuiCompositionViewModel is not { } viewModel)
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
}