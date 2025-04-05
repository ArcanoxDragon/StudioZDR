using ReactiveUI.SourceGenerators;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.ViewModels;

public partial class MainWindowViewModel(IEnumerable<IFeature> allFeatures, IFeatureLauncher featureLauncher) : ViewModelBase
{
	public List<IFeature> Features { get; } = allFeatures.ToList();

	[ReactiveCommand]
	public void LaunchFeature(IFeature feature)
		=> featureLauncher.LaunchFeature(feature);
}