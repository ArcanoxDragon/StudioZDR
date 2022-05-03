using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Framework;

public sealed class ViewModelFactory
{
	private readonly ILifetimeScope scope;

	public ViewModelFactory(ILifetimeScope scope)
	{
		this.scope = scope;
	}

	public T Create<T>()
	where T : ViewModelBase
		=> this.scope.Resolve<T>();

	public ViewModelBase Create(Type viewModelType)
		=> (ViewModelBase) this.scope.Resolve(viewModelType);
}