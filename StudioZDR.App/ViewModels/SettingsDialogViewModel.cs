﻿using Microsoft.Extensions.Options;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Configuration;
using StudioZDR.App.Framework;

namespace StudioZDR.App.ViewModels;

public partial class SettingsDialogViewModel : ViewModelBase
{
	private readonly IWindowContext   windowContext;
	private readonly ISettingsManager settingsManager;
	private readonly IDialogs         dialogs;
	private readonly IFileBrowser     fileBrowser;

	public SettingsDialogViewModel(
		IWindowContext windowContext,
		ISettingsManager settingsManager,
		IDialogs dialogs,
		IFileBrowser fileBrowser,
		IOptionsFactory<ApplicationSettings> settings
	)
	{
		this.windowContext = windowContext;
		this.settingsManager = settingsManager;
		this.dialogs = dialogs;
		this.fileBrowser = fileBrowser;

		RomFsLocation = settings.Create(Options.DefaultName).RomFsLocation ?? string.Empty;
	}

	[Reactive]
	public partial string RomFsLocation { get; set; }

	[ReactiveCommand]
	private async Task BrowseForRomFsAsync()
	{
		var romFsFolder = await this.fileBrowser.OpenFolderAsync("Choose RomFS Folder");

		if (romFsFolder is null)
			return;

		if (!Directory.Exists(romFsFolder))
		{
			await this.dialogs.AlertAsync("Invalid RomFS Folder", "The folder you selected does not exist or is not valid.");
			return;
		}

		var packsExists = Directory.Exists(Path.Join(romFsFolder, "packs"));
		var systemExists = Directory.Exists(Path.Join(romFsFolder, "system"));
		var configExists = File.Exists(Path.Join(romFsFolder, "config.ini"));

		if (!packsExists || !systemExists || !configExists)
		{
			var confirmed = await this.dialogs.ConfirmAsync(
				"Invalid RomFS Folder",
				"The folder you selected does not appear to be a Metroid Dread RomFS folder.\n" +
				"Are you sure you want to choose this folder?",
				"Yes",
				"No");

			if (!confirmed)
				return;
		}

		RomFsLocation = romFsFolder;
	}

	[ReactiveCommand]
	private async Task SaveAsync()
	{
		await this.settingsManager.ModifyAsync(settings => {
			settings.RomFsLocation = RomFsLocation;
		});
		this.windowContext.Close();
	}

	[ReactiveCommand]
	private void Cancel() => this.windowContext.Close();
}