using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Framework;

public sealed class ViewModelFactory(ILifetimeScope scope)
{
	public T Create<T>()
	where T : ViewModelBase
		=> InitializeViewModel(scope.Resolve<T>());

	public ViewModelBase Create(Type viewModelType)
		=> InitializeViewModel((ViewModelBase) scope.Resolve(viewModelType));

	private T InitializeViewModel<T>(T viewModel)
	where T : ViewModelBase
	{
		viewModel.InstallDefaultServices(scope);
		return viewModel;
	}
}