using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;

namespace StudioZDR.App.ViewModels;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
public partial class FeatureWindowViewModel : ViewModelBase
{
	public FeatureWindowViewModel(ViewModelFactory viewModelFactory)
	{
		this._windowTitle = string.Empty;

		// Initialize the view model automatically
		this.WhenAnyValue(m => m.Feature)
			.WhereNotNull()
			.DistinctUntilChanged()
			.Select(feature => viewModelFactory.Create(feature.ViewModelType))
			.ToProperty(this, m => m.FeatureViewModel, out this._featureViewModelHelper);

		// Update window title automatically
		this.WhenAnyValue(m => m.Feature)
			.SelectMany(feature => feature?.WhenAnyValue(f => f.Name) ?? Observable.Return(string.Empty))
			.DistinctUntilChanged()
			.Select(featureName => $"Studio ZDR - {featureName}".TrimEnd(' ', '-'))
			.ToProperty(this, m => m.WindowTitle, out this._windowTitleHelper);

		this.WhenActivated(disposables => {
			Disposable.Create(() => {
				Feature = null;
				OwningWindow = null;
			}).DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial IWindowContext? OwningWindow { get; set; }

	[Reactive]
	public partial IFeature? Feature { get; set; }

	[ObservableAsProperty]
	public partial ViewModelBase? FeatureViewModel { get; }

	[ObservableAsProperty(InitialValue = "")]
	public partial string WindowTitle { get; }
}