using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
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
		get => field ?? throw new InvalidOperationException("ViewModel has not been initialized");
		private set;
	}

	public void InstallDefaultServices(ILifetimeScope scope)
	{
		Dialogs = scope.Resolve<IDialogs>();
	}
}