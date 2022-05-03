using System.Reactive.Disposables;
using ReactiveUI;
using StudioZDR.UI.Avalonia.Framework;

namespace StudioZDR.UI.Avalonia.Views;

public class BaseWindow<TViewModel> : ReactiveWindow<TViewModel>
where TViewModel : class
{
	private ILifetimeScope? lifetimeScope;

	public BaseWindow()
	{
		this.WhenActivated(disposables => {
			// Create a new lifetime scope for the life of this window
			this.lifetimeScope = App.Container.BeginLifetimeScope(ConfigureServicesInternal).DisposeWith(disposables);

			// Also set the scope to null when disposing
			Disposable.Create(() => this.lifetimeScope = null).DisposeWith(disposables);

			// Populate the ViewModel from the scope
			ViewModel = GetRequiredService<TViewModel>();
		});
	}

	protected T? GetService<T>()
	where T : class
		=> this.lifetimeScope?.ResolveOptional<T>();

	protected T GetRequiredService<T>()
	where T : class
	{
		if (this.lifetimeScope is null)
			throw new InvalidOperationException("Services cannot be resolved for an inactive window");

		return this.lifetimeScope.Resolve<T>();
	}

	protected void ConfigureServicesInternal(ContainerBuilder builder)
	{
		// Register a WindowContext for this Window
		builder.RegisterInstance(new WindowContext(this));

		// Register the ViewModel type
		builder.RegisterType<TViewModel>().SingleInstance();

		ConfigureServices(builder);
	}

	protected virtual void ConfigureServices(ContainerBuilder builder) { }
}