using StudioZDR.App.Framework;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<AvaloniaFeatureLauncher>().As<IFeatureLauncher>().SingleInstance();
		builder.RegisterType<AvaloniaDialogs>().As<IDialogs>();
		builder.RegisterType<AvaloniaFileBrowser>().As<IFileBrowser>();
	}
}