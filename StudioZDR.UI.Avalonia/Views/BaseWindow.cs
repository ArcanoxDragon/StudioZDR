﻿using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using ReactiveUI;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;
using StudioZDR.UI.Avalonia.Framework;

namespace StudioZDR.UI.Avalonia.Views;

public class BaseWindow<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	TViewModel
> : ReactiveWindow<TViewModel>, ILifetimeScopeRoot
where TViewModel : ViewModelBase
{
	private ILifetimeScope? lifetimeScope;

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	public BaseWindow()
	{
		this.WhenActivated(disposables => {
			// Create a new lifetime scope for the life of this window
			this.lifetimeScope = App.Container.BeginLifetimeScope(ConfigureServicesInternal).DisposeWith(disposables);

			// Populate the ViewModel from the scope
			ViewModel = InitializeViewModel();

			Disposable.Create(() => {
				// Set the ViewModel and scope to null when disposing
				ViewModel = null;
				this.lifetimeScope = null;
			}).DisposeWith(disposables);
		});
	}

	public ILifetimeScope LifetimeScope
		=> this.lifetimeScope ??
		   throw new InvalidOperationException($"{nameof(LifetimeScope)} can only be accessed while the window is active");

	protected virtual TViewModel InitializeViewModel()
	{
		var viewModelFactory = GetRequiredService<ViewModelFactory>();
		var viewModel = viewModelFactory.Create<TViewModel>();

		viewModel.InstallDefaultServices(this.lifetimeScope!);

		return viewModel;
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
		builder.RegisterInstance(new WindowContext(this)).AsSelf().As<IWindowContext>();

		// Register the ViewModel type
		builder.RegisterType<TViewModel>().SingleInstance();

		ConfigureServices(builder);
	}

	protected virtual void ConfigureServices(ContainerBuilder builder) { }
}