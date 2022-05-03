using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	private readonly IFeatureLauncher featureLauncher;

	public MainWindowViewModel(IEnumerable<IFeature> allFeatures, IFeatureLauncher featureLauncher)
	{
		this.featureLauncher = featureLauncher;
		Features = allFeatures.ToList();
	}

	public List<IFeature> Features { get; }

	public void LaunchFeature(IFeature feature)
		=> this.featureLauncher.LaunchFeature(feature);
}