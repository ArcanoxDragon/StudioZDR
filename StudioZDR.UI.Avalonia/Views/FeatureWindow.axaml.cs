using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using StudioZDR.App.Features;
using StudioZDR.App.Framework;
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

		private bool inWindowOnClosing;

		private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
		{
			try
			{
				// TODO: Should be able to use e.IsProgrammatic, but CaptionButtons.OnClose incorrectly calls with "isProgrammatic: true"
				if (this.inWindowOnClosing)
					// If we recurse, skip any checks - that means we're closing after confirmation
					return;

				if (ViewModel?.FeatureViewModel is not IBlockCloseWhenDirty { IsDirty: true } blockCloseWhenDirty)
					// No need to check for dirty state
					return;

				// Immediately cancel the close event, since we need to do some async work before closing
				e.Cancel = true;
				this.inWindowOnClosing = true;

				var shouldClose = await blockCloseWhenDirty.ConfirmCloseWhenDirtyAsync();

				if (shouldClose)
					Close(); // Trigger the close again - the "IsProgrammatic" check above prevents infinite recursion
			}
			catch (Exception ex)
			{
				ViewModel?.Logger.LogError(ex, "Caught unhandled exception in {Window_OnClosing}", nameof(Window_OnClosing));
			}
			finally
			{
				this.inWindowOnClosing = false;
			}
		}
	}
}