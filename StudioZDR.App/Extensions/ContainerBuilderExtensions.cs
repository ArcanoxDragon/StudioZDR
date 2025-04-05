using Microsoft.Extensions.Options;
using StudioZDR.App.Configuration;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.Extensions;

public static class ContainerBuilderExtensions
{
	public static void UseStudioZdrApp(this ContainerBuilder builder)
	{
		// Register framework services
		builder.Register(c => new ViewModelFactory(c.Resolve<ILifetimeScope>())).InstancePerLifetimeScope();
		builder.RegisterType<SettingsManager>()
			.SingleInstance()
			.As<ISettingsManager>()
			.As<IConfigureOptions<ApplicationSettings>>()
			.As<IOptionsChangeTokenSource<ApplicationSettings>>();

		// Automatically register all feature modules
		builder.RegisterAssemblyModules<FeatureModule>(typeof(FeatureModule).Assembly);
	}
}