using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Types.DreadTypes;
using Microsoft.Extensions.Logging;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Extensions;
using StudioZDR.App.Framework.Dialogs;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public partial class SpriteGridPropertiesViewModel : DisplayObjectPropertiesViewModel
{
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", 
								  Justification = "WhenAnyValue will only ever reference properties from TrimmerRootAssembly")]
	public SpriteGridPropertiesViewModel()
	{
		ChooseSpriteCommand
			.HandleExceptionsWith(ex => {
				Logger.LogError(ex, "An exception was thrown while displaying the sprite picker dialog");
				return Dialogs.Alert(
					"Error",
					"Could not display the sprite chooser:\n\n" +
					$"{ex.GetType().Name}: {ex.Message}",
					"Dismiss");
			})
			.Subscribe();

		this.WhenAnyValue(m => m.SpriteName)
			.Subscribe(spriteName => SetAllValues((GUI__CSpriteGrid sprite) => sprite.CellDefaultSpriteSheetItem, spriteName));

		this.WhenAnyValue(m => m.CellSizeX)
			.Subscribe(size => SetAllValues((GUI__CSpriteGrid sprite) => sprite.CellSizeX, size));

		this.WhenAnyValue(m => m.CellSizeY)
			.Subscribe(size => SetAllValues((GUI__CSpriteGrid sprite) => sprite.CellSizeY, size));

		this.WhenAnyValue(m => m.CellCountX)
			.Subscribe(count => SetAllValues((GUI__CSpriteGrid sprite) => sprite.GridSizeX, count));

		this.WhenAnyValue(m => m.CellCountY)
			.Subscribe(count => SetAllValues((GUI__CSpriteGrid sprite) => sprite.GridSizeY, count));
	}

	#region Sprite

	[Reactive]
	public partial string? SpriteName { get; set; }

	[Reactive]
	public partial string? SpriteNameWatermark { get; set; }

	#endregion

	#region CellSize

	[Reactive]
	public partial float? CellSizeX { get; set; }

	[Reactive]
	public partial string? CellSizeXWatermark { get; set; }

	[Reactive]
	public partial float? CellSizeY { get; set; }

	[Reactive]
	public partial string? CellSizeYWatermark { get; set; }

	#endregion

	#region CellCount

	[Reactive]
	public partial int? CellCountX { get; set; }

	[Reactive]
	public partial string? CellCountXWatermark { get; set; }

	[Reactive]
	public partial int? CellCountY { get; set; }

	[Reactive]
	public partial string? CellCountYWatermark { get; set; }

	#endregion

	[ReactiveCommand]
	public async Task ChooseSpriteAsync()
	{
		if (Nodes is null)
			return;

		var options = new ChooseSpriteOptions();
		var distinctSpriteNames = Nodes
			.Select(n => ( n.DisplayObject as GUI__CSpriteGrid )?.CellDefaultSpriteSheetItem)
			.WhereNotNull()
			.Distinct()
			.ToList();

		if (distinctSpriteNames.Count == 1 && distinctSpriteNames[0].Split('/') is [var spriteSheetName, var spriteName])
		{
			options.AutoSelectSpriteSheet = spriteSheetName;
			options.AutoSelectSprite = spriteName;
		}

		var result = await Dialogs.ChooseSpriteAsync(
			"Choose New Sprite",
			"Select a new sprite to use for the selected objects",
			options);

		if (result is null)
			return;

		SetAllValues((GUI__CSpriteGrid sprite) => sprite.CellDefaultSpriteSheetItem, result);
	}

	protected override void ResetValues()
	{
		base.ResetValues();

		SpriteName = null;
		SpriteNameWatermark = null;
		CellSizeX = null;
		CellSizeXWatermark = null;
		CellSizeY = null;
		CellSizeYWatermark = null;
		CellCountX = null;
		CellCountXWatermark = null;
		CellCountY = null;
		CellCountYWatermark = null;
	}

	protected override void RefreshValuesFromObject(GUI__CDisplayObject? obj, bool firstObject)
	{
		base.RefreshValuesFromObject(obj, firstObject);

		var sprite = obj as GUI__CSpriteGrid;
		var cellSizeX = sprite?.CellSizeX ?? 0f;
		var cellSizeY = sprite?.CellSizeY ?? 0f;
		var cellCountX = sprite?.GridSizeX ?? 0;
		var cellCountY = sprite?.GridSizeY ?? 0;

		if (firstObject)
		{
			SpriteName = sprite?.CellDefaultSpriteSheetItem;
			CellSizeX = cellSizeX;
			CellSizeY = cellSizeY;
			CellCountX = cellCountX;
			CellCountY = cellCountY;
		}
		else
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (sprite?.CellDefaultSpriteSheetItem != SpriteName)
			{
				SpriteName = null;
				SpriteNameWatermark = MultipleValuesPlaceholder;
			}

			if (cellSizeX != CellSizeX)
			{
				CellSizeX = null;
				CellSizeXWatermark = MultipleValuesPlaceholder;
			}

			if (cellSizeY != CellSizeY)
			{
				CellSizeY = null;
				CellSizeYWatermark = MultipleValuesPlaceholder;
			}

			if (cellCountX != CellCountX)
			{
				CellCountX = null;
				CellCountXWatermark = MultipleValuesPlaceholder;
			}

			if (cellCountY != CellCountY)
			{
				CellCountY = null;
				CellCountYWatermark = MultipleValuesPlaceholder;
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}
	}
}