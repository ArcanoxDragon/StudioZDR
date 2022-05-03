using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StudioZDR.UI.Avalonia.Views;

namespace StudioZDR.UI.Avalonia;

public partial class App : Application
{
	public static IContainer Container { get; set; } = null!;

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = new MainWindow();

		base.OnFrameworkInitializationCompleted();
	}
}