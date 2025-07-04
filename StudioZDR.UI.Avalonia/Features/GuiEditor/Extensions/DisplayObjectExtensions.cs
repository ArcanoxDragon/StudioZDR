﻿using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Features.GuiEditor.ViewModels;
using StudioZDR.UI.Avalonia.Extensions;

namespace StudioZDR.UI.Avalonia.Features.GuiEditor.Extensions;

internal static class DisplayObjectExtensions
{
	public static Rect GetOverallBounds(this IEnumerable<GuiCompositionNodeViewModel> nodes, Rect screenBounds)
	{
		Dictionary<GuiCompositionNodeViewModel, Rect> knownBounds = [];
		IEnumerable<Rect> allNodeBounds = nodes.Select(GetBounds);

		return allNodeBounds.GetOverallBoundingBox();

		Rect GetBounds(GuiCompositionNodeViewModel node)
		{
			if (!knownBounds.TryGetValue(node, out var bounds))
			{
				Rect parentBounds = node.Parent is { } parent ? GetBounds(parent) : screenBounds;

				bounds = node.DisplayObject?.GetDisplayObjectBounds(screenBounds, parentBounds) ?? new Rect();
				knownBounds.Add(node, bounds);
			}

			return bounds;
		}
	}

	public static Rect GetDisplayObjectBounds(this GuiCompositionNodeViewModel node, Rect screenBounds, out Point origin)
	{
		if (node.DisplayObject is not { } displayObject)
		{
			origin = default;
			return new Rect();
		}

		Rect parentBounds = node.Parent?.GetDisplayObjectBounds(screenBounds, out _) ?? screenBounds;

		return displayObject.GetDisplayObjectBounds(screenBounds, parentBounds, out origin);
	}

	public static Rect GetDisplayObjectBounds(this GUI__CDisplayObject obj, Rect screenBounds, Rect parentBounds)
		=> GetDisplayObjectBounds(obj, screenBounds, parentBounds, out _);

	public static Rect GetDisplayObjectBounds(this GUI__CDisplayObject obj, Rect screenBounds, Rect parentBounds, out Point origin)
	{
		var scaleX = obj.ScaleX ?? 1f;
		var scaleY = obj.ScaleY ?? 1f;
		var objWidth = ( obj.SizeX ?? 1.0 ) * screenBounds.Width * scaleX;
		var objHeight = ( obj.SizeY ?? 1.0 ) * screenBounds.Height * scaleY;
		var refX = parentBounds.X;
		var refY = parentBounds.Y;
		double boundsX, boundsY;
		double originX, originY;

		if (obj.RightX.HasValue)
		{
			var rightRefX = refX + parentBounds.Width;

			originX = rightRefX - ( obj.RightX.Value * screenBounds.Width );
			boundsX = originX - objWidth;
		}
		else if (obj.CenterX.HasValue)
		{
			var centerRefX = refX + ( 0.5 * parentBounds.Width );

			originX = centerRefX + ( obj.CenterX.Value * screenBounds.Width );
			boundsX = originX - ( 0.5 * objWidth );
		}
		else
		{
			originX = refX + ( ( obj.LeftX ?? obj.X ?? 0.0 ) * screenBounds.Width );
			boundsX = originX;
		}

		if (obj.BottomY.HasValue)
		{
			var bottomRefY = refY + parentBounds.Height;

			originY = bottomRefY - ( obj.BottomY.Value * screenBounds.Height );
			boundsY = originY - objHeight;
		}
		else if (obj.CenterY.HasValue)
		{
			var centerRefY = refY + ( 0.5 * parentBounds.Height );

			originY = centerRefY + ( obj.CenterY.Value * screenBounds.Height );
			boundsY = originY - ( 0.5 * objHeight );
		}
		else
		{
			originY = refY + ( ( obj.TopY ?? obj.Y ?? 0.0 ) * screenBounds.Height );
			boundsY = originY;
		}

		origin = new Point(originX, originY);
		return new Rect(boundsX, boundsY, objWidth, objHeight);
	}

	public static void MoveOriginTo(this GUI__CDisplayObject obj, Rect screenBounds, Rect parentBounds, Point origin)
	{
		var refX = parentBounds.X;
		var refY = parentBounds.Y;

		// Move X coordinate
		if (obj.RightX.HasValue)
		{
			var rightRefX = refX + parentBounds.Width;

			obj.RightX = (float) ( ( rightRefX - origin.X ) / screenBounds.Width );
		}
		else if (obj.CenterX.HasValue)
		{
			var centerRefX = refX + ( 0.5 * parentBounds.Width );

			obj.CenterX = (float) ( ( origin.X - centerRefX ) / screenBounds.Width );
		}
		else
		{
			float newLeft = (float) ( ( origin.X - refX ) / screenBounds.Width );

			if (obj.LeftX.HasValue)
				obj.LeftX = newLeft;
			else
				obj.X = newLeft;
		}

		// Move Y coordinate
		if (obj.BottomY.HasValue)
		{
			var bottomRefY = refY + parentBounds.Height;

			obj.BottomY = (float) ( ( bottomRefY - origin.Y ) / screenBounds.Height );
		}
		else if (obj.CenterY.HasValue)
		{
			var centerRefY = refY + ( 0.5 * parentBounds.Height );

			obj.CenterY = (float) ( ( origin.Y - centerRefY ) / screenBounds.Height );
		}
		else
		{
			float newTop = (float) ( ( origin.Y - refY ) / screenBounds.Height );

			if (obj.TopY.HasValue)
				obj.TopY = newTop;
			else
				obj.Y = newTop;
		}
	}
}