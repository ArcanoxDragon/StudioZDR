using StudioZDR.App.Features;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Views;

public partial class MainWindow : BaseWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}

	public void HandleLaunchFeature(object? parameter)
	{
		if (parameter is IFeature feature)
			ViewModel?.LaunchFeature(feature);
	}
}