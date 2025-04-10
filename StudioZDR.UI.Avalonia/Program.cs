﻿using Autofac.Extensions.DependencyInjection;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using ReactiveUI;
using Serilog;
using Splat.Autofac;
using StudioZDR.App.Extensions;
using StudioZDR.App.Utility;
using StudioZDR.UI.Avalonia.Framework;
using StudioZDR.UI.Avalonia.Icons;

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
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "ReactiveUI assembly is a TrimmerRootAssembly, and thus its types will be preserved")]
	public static AppBuilder BuildAvaloniaApp()
	{
		// Set up .NET DI container
		var serviceCollection = new ServiceCollection();

		serviceCollection.AddLogging(logging => {
			var logPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ZdrConstants.Application.AppDataFolderName, "Logs", "application.log");
			var serilogLogger = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Debug()
#else
				.MinimumLevel.Information()
#endif
				.Enrich.FromLogContext()
				.WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
				.CreateLogger();

			logging.AddConsole();
			logging.AddSerilog(serilogLogger);
		});
		serviceCollection.AddOptions();

		// Connect Autofac and Splat/ReactiveUI
		var builder = new ContainerBuilder();
		var resolver = builder.UseAutofacDependencyResolver();

		resolver.InitializeReactiveUI();
		builder.Populate(serviceCollection);
		builder.UseStudioZdrApp();
		builder.RegisterModule<AvaloniaModule>();

		IconProvider.Current
			.Register(new FontAwesomeKitIconProvider());

		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace() // TODO: LogToDelegate into ILogger once Avalonia updates
			.UseReactiveUI()
			.With(new Win32PlatformOptions {
				WinUICompositionBackdropCornerRadius = 8,
			})
			.AfterPlatformServicesSetup(_ => {
				App.Container = builder.Build();

				resolver.SetLifetimeScope(App.Container);
			});
	}
}