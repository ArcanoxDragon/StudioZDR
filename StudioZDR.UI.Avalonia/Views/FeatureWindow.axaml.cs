using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;
using StudioZDR.App.Features;
using StudioZDR.App.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;
using StudioZDR.UI.Avalonia.Framework;

namespace StudioZDR.UI.Avalonia.Views
{
	public partial class FeatureWindow : BaseWindow<FeatureWindowViewModel>
	{
		private readonly IFeature? feature;

		private bool centerAfterBoundsChange;

		public FeatureWindow()
		{
			InitializeComponent();
		}

		public FeatureWindow(IFeature feature) : this()
		{
			this.feature = feature;

			// Center the window in the screen any time the size is automatically changed
			this.WhenAnyValue(w => w.Bounds)
				.Subscribe(_ => {
					if (this.centerAfterBoundsChange)
						this.CenterInScreen();

					this.centerAfterBoundsChange = false;
				});

			this.WhenActivated(disposables => {
				// Apply a feature's preferred view size when the feature is loaded
				this.WhenAnyValue(w => w.ViewModel)
					.SelectMany(viewModel => viewModel?.WhenAnyValue(vm => vm.Feature) ?? Observable.Never<IFeature>())
					.DistinctUntilChanged()
					.Subscribe(_ => OnFeatureChanged())
					.DisposeWith(disposables);
			});
		}

		protected override FeatureWindowViewModel InitializeViewModel()
		{
			var viewModel = base.InitializeViewModel();
			viewModel.Feature = this.feature;
			return viewModel;
		}

		private void OnFeatureChanged()
		{
			this.centerAfterBoundsChange = true;

			foreach (var logicalChild in LogicalChildren)
			{
				if (logicalChild is not UserControl userControl)
					continue;

				if (userControl.IsSet(FeatureView.PreferredSizeProperty))
				{
					var preferredSize = userControl.GetValue(FeatureView.PreferredSizeProperty);

					Bounds = new Rect(Bounds.Position, preferredSize);
				}
			}
		}
	}
}