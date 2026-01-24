using MercuryEngine.Data.Types.DreadTypes;
using Microsoft.Extensions.Logging;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Extensions;
using StudioZDR.App.Framework.Dialogs;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public partial class SpritePropertiesViewModel : DisplayObjectPropertiesViewModel
{
	public SpritePropertiesViewModel()
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
			.Subscribe(spriteName => SetAllValues((GUI__CSprite sprite) => sprite.SpriteSheetItem, spriteName));

		this.WhenAnyValue(m => m.AutoSize)
			.Subscribe(autoSize => SetAllValues((GUI__CSprite sprite) => sprite.Autosize, autoSize));
	}

	#region Sprite

	[Reactive]
	public partial string? SpriteName { get; set; }

	[Reactive]
	public partial string? SpriteNameWatermark { get; set; }

	#endregion

	#region AutoSize

	[Reactive]
	public partial bool? AutoSize { get; set; }

	#endregion

	[ReactiveCommand]
	public async Task ChooseSpriteAsync()
	{
		if (Nodes is null)
			return;

		var options = new ChooseSpriteOptions();
		var distinctSpriteNames = Nodes
			.Select(n => ( n.DisplayObject as GUI__CSprite )?.SpriteSheetItem)
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

		SetAllValues((GUI__CSprite sprite) => sprite.SpriteSheetItem, result);
	}

	protected override void ResetValues()
	{
		base.ResetValues();

		SpriteName = null;
		SpriteNameWatermark = null;
		AutoSize = true;
	}

	protected override void RefreshValuesFromObject(GUI__CDisplayObject? obj, bool firstObject)
	{
		base.RefreshValuesFromObject(obj, firstObject);

		GUI__CSprite? sprite = obj as GUI__CSprite;
		var spriteSheetItem = sprite?.SpriteSheetItem;
		bool autoSize = sprite?.Autosize ?? true;

		if (firstObject)
		{
			SpriteName = spriteSheetItem;
			AutoSize = autoSize;
		}
		else
		{
			if (spriteSheetItem != SpriteName)
			{
				SpriteName = null;
				SpriteNameWatermark = MultipleValuesPlaceholder;
			}

			if (autoSize != AutoSize)
				AutoSize = null;
		}
	}
}