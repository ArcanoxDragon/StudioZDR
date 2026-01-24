using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StudioZDR.App.Framework.Dialogs;
using StudioZDR.UI.Avalonia.Views.Dialogs.ExtraOptions;

namespace StudioZDR.UI.Avalonia.Framework.Dialogs;

internal class DialogOptionDataTemplate : IDataTemplate
{
	public Control Build(object? param)
		=> param switch {
			ExtraDialogCheckboxOption => new ExtraCheckboxOption(),

			_ => new TextBlock { Text = $"Unknown option type: {param?.GetType().FullName}" },
		};

	public bool Match(object? data)
		=> data is ExtraDialogOption;
}