using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Features.GuiEditor.HelperTypes;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal interface IDisplayObjectRenderer
{
	void RenderObject(DreadGuiDrawContext context, GUI__CDisplayObject obj, GuiTransform parentTransform);
}