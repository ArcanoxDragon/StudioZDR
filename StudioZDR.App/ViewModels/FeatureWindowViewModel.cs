using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.ViewModels;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
public class FeatureWindowViewModel : ViewModelBase, IActivatableViewModel
{
	public FeatureWindowViewModel(ViewModelFactory viewModelFactory)
	{
		this.WhenActivated(disposables => {
			// Initialize the view model automatically
			this.WhenAnyValue(m => m.Feature)
				.WhereNotNull()
				.DistinctUntilChanged()
				.Select(feature => viewModelFactory.Create(feature.ViewModelType))
				.ToPropertyEx(this, m => m.FeatureViewModel)
				.DisposeWith(disposables);

			// Hook up window with feature's ViewModel if able
			this.WhenAnyValue(m => m.FeatureViewModel)
				.WhereNotNull()
				.OfType<IWindowAware>()
				.CombineLatest(this.WhenAnyValue(m => m.OwningWindow))
				.Subscribe(t => t.First.ParentWindow = t.Second)
				.DisposeWith(disposables);
		});
	}

	public ViewModelActivator Activator { get; } = new();

	[Reactive]
	public IWindow? OwningWindow { get; set; }

	[Reactive]
	public IFeature? Feature { get; set; }

	[ObservableAsProperty]
	public ViewModelBase? FeatureViewModel { get; }
}