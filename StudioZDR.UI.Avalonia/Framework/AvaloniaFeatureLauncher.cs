using Avalonia.Controls.ApplicationLifetimes;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;
using StudioZDR.UI.Avalonia.Views;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaFeatureLauncher : IFeatureLauncher
{
	public void LaunchFeature(IFeature feature)
	{
		if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: MainWindow mainWindow })
			return;

		var featureWindow = new FeatureWindow(feature);

		featureWindow.Show(mainWindow);
	}
}