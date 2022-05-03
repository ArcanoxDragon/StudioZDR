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
		});
	}

	public ViewModelActivator Activator { get; } = new();

	[Reactive]
	public IFeature? Feature { get; set; }

	[ObservableAsProperty]
	public ViewModelBase? FeatureViewModel { get; }
}