using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.ViewModels;

public class MainWindowViewModel(IEnumerable<IFeature> allFeatures, IFeatureLauncher featureLauncher) : ViewModelBase
{
	public List<IFeature> Features { get; } = allFeatures.ToList();

	public void LaunchFeature(IFeature feature)
		=> featureLauncher.LaunchFeature(feature);
}