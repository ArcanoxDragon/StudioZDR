using Avalonia.Controls;
using ReactiveUI;
using Splat.Autofac;
using StudioZDR.App.Extensions;
using StudioZDR.UI.Avalonia.Framework;

namespace StudioZDR.UI.Avalonia;

internal class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
		=> BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
	{
		// Connect Autofac and Splat/ReactiveUI
		var builder = new ContainerBuilder();
		var resolver = builder.UseAutofacDependencyResolver();

		resolver.InitializeReactiveUI();
		builder.UseStudioZdrApp();
		builder.RegisterModule<AvaloniaModule>();

		return AppBuilder.Configure<App>()
						 .UsePlatformDetect()
						 .LogToTrace()
						 .UseReactiveUI()
						 .AfterPlatformServicesSetup(_ => {
							 App.Container = builder.Build();

							 resolver.SetLifetimeScope(App.Container);
						 });
	}
}