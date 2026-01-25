using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace StudioZDR.App.ViewModels;

public partial class ListBoxDialogViewModel : ViewModelBase
{
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
								  Justification = "WhenAnyValue will only ever reference properties from TrimmerRootAssembly")]
	public ListBoxDialogViewModel()
	{
		this.WhenAnyValue(m => m.SelectedItem, (object? i) => i is not null)
			.ToProperty(this, m => m.HasSelection, out this._hasSelectionHelper);
	}

	[Reactive]
	public partial object? SelectedItem { get; set; }

	[ObservableAsProperty]
	public partial bool HasSelection { get; }
}