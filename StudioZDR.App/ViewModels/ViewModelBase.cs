namespace StudioZDR.App.ViewModels;

public class ViewModelBase : ReactiveObject, IActivatableViewModel
{
	public ViewModelActivator Activator { get; } = new();
}