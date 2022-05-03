using StudioZDR.App.Features;

namespace StudioZDR.App.Framework;

public interface IFeatureLauncher
{
	void LaunchFeature(IFeature feature);
}