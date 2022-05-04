using Material.Icons;

namespace StudioZDR.App.Features;

public interface IFeature
{
	string           Name          { get; }
	string           Description   { get; }
	MaterialIconKind IconKind      { get; }
	Type             ViewModelType { get; }
}