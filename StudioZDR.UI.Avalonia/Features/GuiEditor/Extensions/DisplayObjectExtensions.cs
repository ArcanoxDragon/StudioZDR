using System.Drawing;
using StudioZDR.App.Extensions;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;

internal static class DisplayObjectExtensions
{
	public static RectangleF GetOverallBounds(this IEnumerable<GuiCompositionNodeViewModel> nodes, RectangleF screenBounds)
	{
		Dictionary<GuiCompositionNodeViewModel, GuiTransform> knownBounds = [];
		IEnumerable<GuiTransform> allNodeTransforms = nodes.Select(GetTransformCached);
		IEnumerable<RectangleF> allNodeTransformedBounds = allNodeTransforms.Select(b => b.AxisAlignedBoundingBox);

		return allNodeTransformedBounds.GetOverallBoundingBox();

		GuiTransform GetTransformCached(GuiCompositionNodeViewModel node)
		{
			if (!knownBounds.TryGetValue(node, out var bounds))
			{
				GuiTransform parentTransform = node.Parent is { } parent
					? GetTransformCached(parent)
					: GuiTransform.CreateDefault(screenBounds);

				bounds = node.DisplayObject?.GetTransform(parentTransform) ?? GuiTransform.CreateEmpty(screenBounds);
				knownBounds.Add(node, bounds);
			}

			return bounds;
		}
	}

	public static GuiTransform GetTransform(this GuiCompositionNodeViewModel node, RectangleF screenBounds)
	{
		if (node.DisplayObject is not { } displayObject)
			return GuiTransform.CreateEmpty(screenBounds);

		var parentBounds = node.Parent?.GetTransform(screenBounds) ?? GuiTransform.CreateDefault(screenBounds);

		return displayObject.GetTransform(parentBounds);
	}
}