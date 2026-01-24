using ReactiveUI.SourceGenerators;

namespace StudioZDR.App.Framework.Dialogs;

public abstract partial class ExtraDialogOption : ReactiveObject
{
	protected ExtraDialogOption(string text, string? description = null)
	{
		Text = text;
		Description = description;
	}

	[Reactive]
	public partial string Text { get; set; }

	[Reactive]
	public partial string? Description { get; set; }
}