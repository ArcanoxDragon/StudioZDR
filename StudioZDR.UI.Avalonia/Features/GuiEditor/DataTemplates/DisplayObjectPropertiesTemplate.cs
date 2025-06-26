using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MercuryEngine.Data.Types.DreadTypes;
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

		if (AllHaveType<GUI__CLabel>())
			return BuildControl<LabelProperties>();
		if (AllHaveType<GUI__CSprite>())
			return BuildControl<SpriteProperties>();
		if (AllHaveType<GUI__CSpriteGrid>())
			return BuildControl<SpriteGridProperties>();

		return BuildControl<DisplayObjectProperties>();

		bool AllHaveType<TDisplayObject>()
		where TDisplayObject : GUI__CDisplayObject
			=> nodes.Count > 0 && nodes.All(n => n.DisplayObject is TDisplayObject);

		TControl BuildControl<TControl>()
		where TControl : UserControl, IDisplayObjectPropertiesControl, new()
			=> new() {
				DataContext = nodes,
				Editor = Editor,
			};
	}

	public bool Match(object? data)
		=> data is null or IList<GuiCompositionNodeViewModel>;
}