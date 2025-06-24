using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Extensions;
using StudioZDR.App.Rendering;

namespace StudioZDR.App.ViewModels;

public partial class SpritePickerDialogViewModel : ViewModelBase
{
	public SpritePickerDialogViewModel(ISpriteSheetManager? spriteSheetManager = null, ILogger<SpritePickerDialogViewModel>? logger = null)
	{
		this._spriteSheets = [];

		LoadSpriteSheetsCommand
			.HandleExceptionsWith(ex => {
				logger?.LogError(ex, "An exception was thrown while picking a sprite");
				return Dialogs.Alert(
					"Error",
					"Could not pick a sprite:\n\n" +
					$"{ex.GetType().Name}: {ex.Message}",
					"Dismiss");
			});

		this._availableSprites = [];

		this.WhenAnyValue(m => m.SpriteSheets)
			.Subscribe(_ => SelectedSpriteSheet = null);

		this.WhenAnyValue(m => m.SelectedSpriteSheet)
			.Subscribe(_ => SelectedSprite = null);

		this.WhenAnyValue(m => m.SelectedSpriteSheet)
			.Select(sheet => {
				if (sheet is null)
					return [];

				return sheet.GetAllSpriteNames().Select(name => new SpriteViewModel(sheet, name)).ToList();
			})
			.ToProperty(this, m => m.AvailableSprites, out this._availableSpritesHelper, initialValue: this._availableSprites);

		this.WhenAnyValue(m => m.SelectedSprite, (SpriteViewModel? s) => s is not null)
			.ToProperty(this, m => m.HasSelection, out this._hasSelectionHelper);

		this.WhenActivated(disposables => {
			if (spriteSheetManager != null)
			{
				LoadSpriteSheetsCommand.Execute(spriteSheetManager)
					.Subscribe(spriteSheets => SpriteSheets = spriteSheets)
					.DisposeWith(disposables);
			}
		});
	}

	[Reactive]
	public partial List<ISpriteSheet> SpriteSheets { get; set; }

	[Reactive]
	public partial ISpriteSheet? SelectedSpriteSheet { get; set; }

	[ObservableAsProperty]
	public partial List<SpriteViewModel> AvailableSprites { get; }

	[Reactive]
	public partial SpriteViewModel? SelectedSprite { get; set; }

	[ObservableAsProperty]
	public partial bool HasSelection { get; }

	[ReactiveCommand]
	private async Task<List<ISpriteSheet>> LoadSpriteSheetsAsync(ISpriteSheetManager spriteSheetManager)
	{
		var spriteSheetNames = spriteSheetManager.GetAllSpriteSheetNamesAsync();

		return await spriteSheetNames.Select(spriteSheetManager.GetSpriteSheet).ToListAsync();
	}
}