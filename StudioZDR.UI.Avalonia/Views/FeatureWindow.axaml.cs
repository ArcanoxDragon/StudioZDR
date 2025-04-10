using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.LogicalTree;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Trimming",
			"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
			Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
		public FeatureWindow(IFeature feature) : this()
		{
			this.feature = feature;

			// Center the window in the screen any time the size is automatically changed
			this.WhenAnyValue(w => w.Bounds)
				.Delay(TimeSpan.FromMilliseconds(10))
				.Subscribe(_ => {
					if (Interlocked.Exchange(ref this.centerAfterBoundsChange, false))
						this.CenterInScreen();
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
			foreach (var logicalChild in this.GetLogicalDescendants())
			{
				if (logicalChild is not UserControl userControl)
					continue;

				if (userControl.IsSet(FeatureView.PreferredSizeProperty))
				{
					var preferredSize = userControl.GetValue(FeatureView.PreferredSizeProperty);

					this.centerAfterBoundsChange = true;
					Width = preferredSize.Width;
					Height = preferredSize.Height;
					break;
				}
			}
		}
	}
}