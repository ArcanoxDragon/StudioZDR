using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Views
{
	public partial class FeatureWindow : BaseWindow<FeatureWindowViewModel>, IWindow
	{
		public FeatureWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		public FeatureWindow(IFeature feature) : this()
		{
			this.WhenActivated(disposables => {
				ViewModel!.OwningWindow = this;
				ViewModel!.Feature = feature;

				// Center the window in the screen any time the size changes in the first half second it's open
				this.WhenAnyValue(w => w.Bounds)
					.Take(TimeSpan.FromSeconds(0.5))
					.Subscribe(_ => this.CenterInScreen())
					.DisposeWith(disposables);
			});
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		void IWindow.Close() => Dispatcher.UIThread.Post(Close);
	}
}