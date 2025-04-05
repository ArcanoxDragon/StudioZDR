using ReactiveUI.SourceGenerators;
using StudioZDR.App.ViewModels;
using StudioZDR.UI.Avalonia.Views.Dialogs;

namespace StudioZDR.UI.Avalonia.Views;

public partial class MainWindow : BaseWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}

	[ReactiveCommand]
	private void OpenSettings()
	{
		var dialog = new SettingsDialog();

		dialog.ShowDialog(this);
	}
}