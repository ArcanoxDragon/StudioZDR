using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI.Fody.Helpers;
using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.Features.SaveEditor.Services;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public class SaveEditorViewModel : ViewModelBase, IActivatableViewModel
{
	private readonly IFileBrowser  fileBrowser;
	private readonly IDialogs      dialogs;
	private readonly ProfileLoader profileLoader;

	public SaveEditorViewModel(IFileBrowser fileBrowser, IDialogs dialogs, ProfileLoader profileLoader)
	{
		this.fileBrowser = fileBrowser;
		this.dialogs = dialogs;
		this.profileLoader = profileLoader;

		this.WhenActivated(disposables => {
			var isProfileLoaded = this.WhenAnyValue(m => m.OpenedProfileName, name => !string.IsNullOrEmpty(name));

			OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync).DisposeWith(disposables);
			OpenFileCommand.LoggedCatch(this);

			SaveFileCommand = ReactiveCommand.CreateFromTask<bool>(SaveFileAsync, isProfileLoaded).DisposeWith(disposables);
			SaveFileCommand.LoggedCatch(this);
		});
	}

	public ViewModelActivator Activator { get; } = new();

	[Reactive]
	public SaveProfile? OpenedProfile { get; set; }

	[Reactive]
	public string? OpenedProfileName { get; set; }

	[Reactive]
	public ReactiveCommand<Unit, Unit>? OpenFileCommand { get; set; }

	[Reactive]
	public ReactiveCommand<bool, Unit>? SaveFileCommand { get; set; }

	private async Task<bool> ConfirmUnsavedChangesAsync()
	{
		if (OpenedProfileName is null)
			return true;

		return await this.dialogs.ConfirmAsync(
				   "Unsaved Changes",
				   "You have unsaved changes. If you open another profile, your changes will be lost.\n\nContinue?"
			   );
	}

	private async Task OpenFileAsync()
	{
		if (!await ConfirmUnsavedChangesAsync())
			return;

		var profileFolder = await this.fileBrowser.OpenFolderAsync("Open Profile Folder");

		if (profileFolder is null)
			return;

		try
		{
			OpenedProfile = await this.profileLoader.Load(profileFolder);
			OpenedProfileName = profileFolder;
		}
		catch (Exception ex)
		{
			await this.dialogs.AlertAsync("Error Loading Profile", $"An error occurred while loading the profile:\n\n{ex}");
		}
	}

	private async Task SaveFileAsync(bool saveAs)
	{
		if (saveAs)
			await this.dialogs.AlertAsync("Save File", "Save As selected!");
		else
			await this.dialogs.AlertAsync("Save File", "Save selected!");
	}
}