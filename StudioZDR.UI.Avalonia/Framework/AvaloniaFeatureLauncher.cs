using StudioZDR.App.Features;
using StudioZDR.App.Framework;
using StudioZDR.UI.Avalonia.Views;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaFeatureLauncher(WindowContext windowContext) : IFeatureLauncher
{
	public void LaunchFeature(IFeature feature)
	{
		var featureWindow = new FeatureWindow(feature);

		featureWindow.Show(windowContext.CurrentWindow);
	}
}