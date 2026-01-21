using Avalonia.VisualTree;
using ReactiveUI;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.App.Features.GuiEditor.ViewModels.Properties;
using StudioZDR.UI.Avalonia.Framework;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Views.Properties;

internal partial class DisplayObjectProperties : DisplayObjectProperties<DisplayObjectPropertiesViewModel>
{
	public DisplayObjectProperties()
	{
		InitializeComponent();
	}
}

internal class DisplayObjectProperties<TViewModel> : ReactiveUserControl<TViewModel>, IDisplayObjectPropertiesControl
where TViewModel : DisplayObjectPropertiesViewModel, new()
{
	public static readonly StyledProperty<GuiEditorViewModel?> EditorProperty
		= AvaloniaProperty.Register<GuiCompositionCanvas, GuiEditorViewModel?>(nameof(Editor));

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	public DisplayObjectProperties()
	{
		this.WhenActivated(disposables => {
			if (this.FindAncestorOfType<ILifetimeScopeRoot>() is not { } scopeRoot)
				return;

			if (ViewModel is null)
			{
				var newViewModel = new TViewModel {
					Nodes = DataContext as IList<GuiCompositionNodeViewModel>,
				}.DisposeWith(disposables);

				newViewModel.InstallDefaultServices(scopeRoot.LifetimeScope);

				// Assigning the ViewModel can cause back-and-forth binding assignments to happen,
				// and we very much do NOT want display objects to be updated with the old/"stale"
				// values in the controls that are being re-bound.
				using (newViewModel.SuppressValueUpdates())
					ViewModel = newViewModel;

				ViewModel?.Changes
					.Subscribe(_ => Editor?.StageUndoOperation())
					.DisposeWith(disposables);
			}
		});
	}

	public GuiEditorViewModel? Editor
	{
		get => GetValue(EditorProperty);
		set => SetValue(EditorProperty, value);
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		ViewModel?.Nodes = DataContext as IList<GuiCompositionNodeViewModel>;
	}
}