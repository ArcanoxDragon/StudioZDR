using Avalonia.Controls;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views.Properties;

internal partial class DisplayObjectProperties : ContentControl
{
	public static readonly DirectProperty<DisplayObjectProperties, DisplayObjectPropertiesViewModel?> ViewModelProperty
		= AvaloniaProperty.RegisterDirect<DisplayObjectProperties, DisplayObjectPropertiesViewModel?>(nameof(ViewModel), obj => obj.ViewModel);

	public DisplayObjectProperties()
	{
		InitializeComponent();
	}

	public DisplayObjectPropertiesViewModel? ViewModel
	{
		get;
		private set => SetAndRaise(ViewModelProperty, ref field, value);
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		ViewModel?.Dispose();
		ViewModel = new DisplayObjectPropertiesViewModel {
			Nodes = DataContext as IList<GuiCompositionNodeViewModel>,
		};
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);

		ViewModel?.Dispose();
		ViewModel = null;
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (ViewModel != null)
			ViewModel.Nodes = DataContext as IList<GuiCompositionNodeViewModel>;
	}
}