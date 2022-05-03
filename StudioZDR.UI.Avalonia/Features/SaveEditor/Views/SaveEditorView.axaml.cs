using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using StudioZDR.App.Features.SaveEditor.ViewModels;

namespace StudioZDR.UI.Avalonia.Features.SaveEditor.Views;

public partial class SaveEditorView : ReactiveUserControl<SaveEditorViewModel>
{
	public SaveEditorView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	private void Close()
	{
		if (this.FindAncestorOfType<Window>() is {} window)
			window.Close();
	}
}