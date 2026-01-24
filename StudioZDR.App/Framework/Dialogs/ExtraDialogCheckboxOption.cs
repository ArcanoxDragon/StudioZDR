using ReactiveUI.SourceGenerators;

namespace StudioZDR.App.Framework.Dialogs;

public partial class ExtraDialogCheckboxOption(string text, string? description = null)
	: ExtraDialogOption(text, description)
{
	[Reactive]
	public partial bool Value { get; set; }
}