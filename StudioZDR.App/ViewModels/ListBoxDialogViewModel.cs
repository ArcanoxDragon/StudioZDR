using ReactiveUI.SourceGenerators;

namespace StudioZDR.App.ViewModels;

public partial class ListBoxDialogViewModel : ViewModelBase
{
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