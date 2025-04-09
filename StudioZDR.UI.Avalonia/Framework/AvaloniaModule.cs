using StudioZDR.App.Framework;
using StudioZDR.UI.Avalonia.Rendering;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<AvaloniaFeatureLauncher>().As<IFeatureLauncher>().InstancePerLifetimeScope();
		builder.RegisterType<AvaloniaDialogs>().As<IDialogs>();
		builder.RegisterType<AvaloniaFileBrowser>().As<IFileBrowser>();
		builder.RegisterType<SpriteSheetManager>();
	}
}