using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Views.Properties;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.DataTemplates;

internal class DisplayObjectPropertiesTemplate : AvaloniaObject, IDataTemplate
{
	public static readonly DirectProperty<DisplayObjectPropertiesTemplate, GuiEditorViewModel?> EditorProperty
		= AvaloniaProperty.RegisterDirect<DisplayObjectPropertiesTemplate, GuiEditorViewModel?>(
			nameof(Editor),
			getter: obj => obj.Editor,
			setter: (obj, value) => obj.Editor = value
		);

	public GuiEditorViewModel? Editor { get; set; }

	public Control Build(object? param)
	{
		if (param is not IList<GuiCompositionNodeViewModel> nodes)
			return new DisplayObjectProperties { Editor = Editor };

		// TODO: Other subtypes

		return new DisplayObjectProperties {
			DataContext = nodes,
			Editor = Editor,
		};
	}

	public bool Match(object? data)
		=> data is null or IList<GuiCompositionNodeViewModel>;
}