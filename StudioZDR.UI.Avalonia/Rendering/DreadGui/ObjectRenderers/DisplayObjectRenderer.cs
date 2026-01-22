using System.Diagnostics;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Features.GuiEditor.HelperTypes;

namespace StudioZDR.UI.Avalonia.Rendering.DreadGui.ObjectRenderers;

internal abstract class DisplayObjectRenderer<TDisplayObject> : IDisplayObjectRenderer
where TDisplayObject : GUI__CDisplayObject
{
	public void RenderObject(DreadGuiDrawContext context, GUI__CDisplayObject obj, GuiTransform parentTransform)
	{
		Debug.Assert(obj is TDisplayObject, $"{GetType().Name} invoked for wrong display object type! ({obj.GetType().Name})");

		RenderObjectCore(context, (TDisplayObject) obj, parentTransform);
	}

	protected abstract void RenderObjectCore(DreadGuiDrawContext context, TDisplayObject obj, GuiTransform parentTransform);
}