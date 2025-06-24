using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using Microsoft.Extensions.Logging;
using StudioZDR.App.Framework.Dialogs;

namespace StudioZDR.App.ViewModels;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class ViewModelBase : ReactiveObject, IActivatableViewModel
{
	protected static IScheduler MainThreadScheduler => RxApp.MainThreadScheduler;
	protected static IScheduler TaskPoolScheduler   => RxApp.TaskpoolScheduler;

	public ViewModelActivator Activator { get; } = new();

	[field: MaybeNull]
	public IDialogs Dialogs
	{
		get => field ?? throw NotInitializedException();
		private set;
	}

	[field: MaybeNull]
	public ILogger Logger
	{
		get => field ?? throw NotInitializedException();
		private set;
	}

	public void InstallDefaultServices(ILifetimeScope scope)
	{
		var loggerFactory = scope.Resolve<ILoggerFactory>();

		Dialogs = scope.Resolve<IDialogs>();
		Logger = loggerFactory.CreateLogger(GetType());
	}

	private static InvalidOperationException NotInitializedException()
		=> new("ViewModel has not been initialized");
}