using Material.Icons;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features;

public abstract class FeatureModule : Module, IFeature
{
	public abstract string Name          { get; }
	public abstract string Description   { get; }
	public abstract Type   ViewModelType { get; }

	public virtual MaterialIconKind IconKind => MaterialIconKind.ToyBrick;

	protected sealed override void Load(ContainerBuilder builder)
	{
		ConfigureServices(builder);

		// Add view model type as a service
		builder.RegisterType(ViewModelType);

		// Add self as a service after all other services for this feature
		builder.RegisterInstance(this).As<IFeature>();
	}

	protected virtual void ConfigureServices(ContainerBuilder builder) { }
}

public abstract class FeatureModule<TViewModel> : FeatureModule
where TViewModel : ViewModelBase
{
	public override Type ViewModelType => typeof(TViewModel);
}