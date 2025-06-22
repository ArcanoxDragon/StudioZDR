using System.Reactive.Disposables;
using Avalonia.Controls;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views.Properties;

internal partial class DisplayObjectProperties : ContentControl
{
	public static readonly DirectProperty<DisplayObjectProperties, DisplayObjectPropertiesViewModel?> ViewModelProperty
		= AvaloniaProperty.RegisterDirect<DisplayObjectProperties, DisplayObjectPropertiesViewModel?>(nameof(ViewModel), obj => obj.ViewModel);

	public static readonly StyledProperty<GuiEditorViewModel?> EditorProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, GuiEditorViewModel?>(nameof(Editor));

	private CompositeDisposable? disposables;

	public DisplayObjectProperties()
	{
		InitializeComponent();
	}

	public DisplayObjectPropertiesViewModel? ViewModel
	{
		get;
		private set => SetAndRaise(ViewModelProperty, ref field, value);
	}

	public GuiEditorViewModel? Editor
	{
		get => GetValue(EditorProperty);
		set => SetValue(EditorProperty, value);
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		this.disposables?.Dispose();
		this.disposables = [];

		ViewModel = new DisplayObjectPropertiesViewModel {
			Nodes = DataContext as IList<GuiCompositionNodeViewModel>,
		};
		ViewModel.DisposeWith(this.disposables);

		ViewModel.Changes
			.Subscribe(_ => Editor?.StageUndoOperation())
			.DisposeWith(this.disposables);
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		this.disposables?.Dispose();
		this.disposables = null;
		ViewModel = null;
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		ViewModel?.Nodes = DataContext as IList<GuiCompositionNodeViewModel>;
	}
}