using System.Text.Json.Serialization;
using StudioZDR.App.Configuration;

namespace StudioZDR.App.Features.GuiEditor.Configuration;

public class GuiEditorSettings : IFeatureSettings<GuiEditorSettings>
{
	public static string                Key                  => "GuiEditor";
	public static JsonSerializerContext SerializationContext => SettingsJsonContext.Default;

	public bool MouseSelectionEnabled { get; set; } = true;

	public void CopyTo(GuiEditorSettings other)
	{
		other.MouseSelectionEnabled = MouseSelectionEnabled;
	}
}