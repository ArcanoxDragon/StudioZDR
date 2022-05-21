using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;
using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
public class SaveEditorViewModel : ViewModelBase, IActivatableViewModel, IWindowAware
{
	private readonly IFileBrowser fileBrowser;
	private readonly IDialogs     dialogs;

	public SaveEditorViewModel(IFileBrowser fileBrowser, IDialogs dialogs)
	{
		this.fileBrowser = fileBrowser;
		this.dialogs = dialogs;

		this.WhenActivated(disposables => {
			this.WhenAnyValue(m => m.OpenedProfile)
				.Select(profile => profile != null)
				.ToPropertyEx(this, m => m.IsProfileOpened);

			// Reset "HasChanges" when a new profile is loaded
			this.WhenAnyValue(m => m.OpenedProfile)
				.DistinctUntilChanged()
				.Subscribe(_ => HasChanges = false);

			#region Child ViewModel Connections

			this.WhenAnyValue(m => m.OpenedProfile, p => p is null ? null : new InventoryViewModel(p.Inventory))
				.ToPropertyEx(this, m => m.Inventory);

			#endregion

			#region Change Tracking

			this.WhenAnyValue(m => m.Inventory)
				.WhereNotNull()
				.SelectMany(i => i.WhenAnyPropertyChanged())
				.Subscribe(_ => HasChanges = true)
				.DisposeWith(disposables);

			#endregion

			var isProfileOpened = this.WhenAnyValue(m => m.IsProfileOpened);

			OpenFile = ReactiveCommand.CreateFromTask(OpenFileAsync).DisposeWith(disposables);
			OpenFile.ThrownExceptions.Subscribe(ex => this.dialogs.AlertAsync("Error Opening Profile", $"An error occurred while opening the profile: {ex}"));

			SaveFile = ReactiveCommand.CreateFromTask<bool>(SaveFileAsync, isProfileOpened).DisposeWith(disposables);
			SaveFile.ThrownExceptions.Subscribe(ex => this.dialogs.AlertAsync("Error Saving Profile", $"An error occurred while saving the profile: {ex}"));

			Close = ReactiveCommand.CreateFromTask(CloseAsync).DisposeWith(disposables);

			OpenFile.IsExecuting
					.CombineLatest(SaveFile.IsExecuting, (a, b) => a || b)
					.ToPropertyEx(this, m => m.IsBusy);
		});
	}

	public ViewModelActivator Activator    { get; } = new();
	public IWindow?           ParentWindow { get; set; }

	[Reactive]
	public bool HasChanges { get; private set; }

	[ObservableAsProperty]
	public bool IsBusy { get; }

	#region Profile

	[ObservableAsProperty]
	[MemberNotNullWhen(true, nameof(OpenedProfile))]
	[MemberNotNullWhen(true, nameof(OpenedProfilePath))]
	public bool IsProfileOpened { get; }

	[Reactive]
	public SaveProfile? OpenedProfile { get; set; }

	[Reactive]
	public string? OpenedProfilePath { get; set; }

	#endregion

	#region Child ViewModels

	[ObservableAsProperty]
	public InventoryViewModel? Inventory { get; }

	#endregion

	#region Commands

	[Reactive]
	public ReactiveCommand<Unit, Unit>? OpenFile { get; set; }

	[Reactive]
	public ReactiveCommand<bool, Unit>? SaveFile { get; set; }

	[Reactive]
	public ReactiveCommand<Unit, Unit>? Close { get; set; }

	private async Task<bool> ConfirmUnsavedChangesAsync()
	{
		if (OpenedProfilePath is null || !HasChanges)
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

		OpenedProfile = await SaveProfile.LoadFromAsync(profileFolder);
		OpenedProfilePath = profileFolder;
	}

	private async Task SaveFileAsync(bool saveAs)
	{
		if (!IsProfileOpened)
			return;

		string? profilePath;

		if (saveAs)
		{
			profilePath = await this.fileBrowser.OpenFolderAsync("Choose Save Destination");

			if (profilePath is null)
				return;
		}
		else
		{
			profilePath = OpenedProfilePath;
		}

		await OpenedProfile.SaveAsync(profilePath);
	}

	private async Task CloseAsync()
	{
		if (await ConfirmUnsavedChangesAsync())
			ParentWindow?.Close();
	}

	#endregion
}