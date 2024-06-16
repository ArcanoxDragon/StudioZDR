using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;
using StudioZDR.UI.Avalonia.Framework;

namespace StudioZDR.UI.Avalonia.Views
{
	public partial class FeatureWindow : BaseWindow<FeatureWindowViewModel>, IWindow
	{
		private bool centerAfterBoundsChange;

		public FeatureWindow()
		{
			InitializeComponent();
		}

		public FeatureWindow(IFeature feature) : this()
		{
			this.WhenActivated(disposables => {
				// Center the window in the screen any time the size is automatically changed
				this.WhenAnyValue(w => w.Bounds)
					.Subscribe(_ => {
						if (this.centerAfterBoundsChange)
							this.CenterInScreen();

						this.centerAfterBoundsChange = false;
					})
					.DisposeWith(disposables);

				// Apply a feature's preferred view size when the feature is loaded
				ViewModel!.WhenAnyValue(vm => vm.Feature)
					.WhereNotNull()
					.DistinctUntilChanged()
					.Subscribe(_ => OnFeatureChanged())
					.DisposeWith(disposables);

				ViewModel!.OwningWindow = this;
				ViewModel!.Feature = feature;
			});
		}

		void IWindow.Close() => Dispatcher.UIThread.Post(Close);

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

					Width = preferredSize.Width;
					Height = preferredSize.Height;
				}
			}
		}
	}
}