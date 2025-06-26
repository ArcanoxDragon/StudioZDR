using MercuryEngine.Data.Types.DreadTypes;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal interface IDisplayObjectRenderer
{
	void RenderObject(DreadGuiDrawContext context, GUI__CDisplayObject obj, Rect parentBounds);
}
