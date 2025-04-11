using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Features.GuiEditor.Views.Properties;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.DataTemplates;

internal class DisplayObjectPropertiesTemplate : IDataTemplate
{
	public Control Build(object? param)
	{
		if (param is not IList<GuiCompositionNodeViewModel> nodes)
			return new DisplayObjectProperties();

		// TODO: Other subtypes

		return new DisplayObjectProperties {
			DataContext = nodes,
		};
	}

	public bool Match(object? data)
		=> data is null or IList<GuiCompositionNodeViewModel>;
}