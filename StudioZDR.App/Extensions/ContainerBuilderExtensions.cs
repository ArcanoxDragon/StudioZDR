using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.Extensions;

public static class ContainerBuilderExtensions
{
	public static void UseStudioZdrApp(this ContainerBuilder builder)
	{
		// Register framework services
		builder.Register(c => new ViewModelFactory(c.Resolve<ILifetimeScope>())).InstancePerLifetimeScope();

		// Automatically register all feature modules
		builder.RegisterAssemblyModules<FeatureModule>(typeof(FeatureModule).Assembly);
	}
}