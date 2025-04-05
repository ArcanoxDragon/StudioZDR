using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public partial class SaveEditorViewModel : ViewModelBase
{
	private readonly IFileBrowser   fileBrowser;
	private readonly IDialogs       dialogs;
	private readonly IWindowContext windowContext;

	public SaveEditorViewModel(IFileBrowser fileBrowser, IDialogs dialogs, IWindowContext windowContext)
	{
		this.fileBrowser = fileBrowser;
		this.dialogs = dialogs;
		this.windowContext = windowContext;

		this.WhenAnyValue(m => m.OpenedProfile)
			.Select(profile => profile != null)
			.ToProperty(this, m => m.IsProfileOpened, out this._isProfileOpenedHelper);

		// Reset "HasChanges" when a new profile is loaded
		this.WhenAnyValue(m => m.OpenedProfile)
			.DistinctUntilChanged()
			.Subscribe(_ => HasChanges = false);

		#region Child ViewModel Connections

		this.WhenAnyValue(m => m.OpenedProfile, p => p is null ? null : new InventoryViewModel(p.Inventory))
			.ToProperty(this, m => m.Inventory, out this._inventoryHelper);

		this.WhenAnyValue(m => m.OpenedProfile, p => p is null ? null : new RandovaniaDataViewModel(p.RandovaniaData))
			.ToProperty(this, m => m.RandovaniaData, out this._randovaniaDataHelper);

		this.WhenAnyValue(m => m.OpenedProfile, (SaveProfile? p) => p?.HasRandovaniaData ?? false)
			.ToProperty(this, m => m.HasRandovaniaData, out this._hasRandovaniaDataHelper);

		#endregion

		this.WhenActivated(disposables => {
			#region Change Tracking

			this.WhenAnyValue(m => m.Inventory)
				.WhereNotNull()
				.SelectMany(i => i.WhenAnyPropertyChanged())
				.Subscribe(_ => HasChanges = true)
				.DisposeWith(disposables);

			this.WhenAnyValue(m => m.RandovaniaData)
				.WhereNotNull()
				.SelectMany(rd => rd.WhenAnyPropertyChanged())
				.Subscribe(_ => HasChanges = true)
				.DisposeWith(disposables);

			#endregion

			OpenFileCommand.ThrownExceptions
				.Subscribe(ex => this.dialogs.AlertAsync("Error Opening Profile", $"An error occurred while opening the profile: {ex}"))
				.DisposeWith(disposables);

			SaveFileCommand.ThrownExceptions
				.Subscribe(ex => this.dialogs.AlertAsync("Error Saving Profile", $"An error occurred while saving the profile: {ex}"))
				.DisposeWith(disposables);

			Observable.CombineLatest(OpenFileCommand.IsExecuting, SaveFileCommand.IsExecuting)
				.Select(values => values.Any(v => v))
				.ToProperty(this, m => m.IsBusy, out this._isBusyHelper)
				.DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial bool HasChanges { get; private set; }

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool IsBusy { get; }

	#region Profile

	[ObservableAsProperty]
	[MemberNotNullWhen(true, nameof(OpenedProfile))]
	[MemberNotNullWhen(true, nameof(OpenedProfilePath))]
	public partial bool IsProfileOpened { get; }

	[Reactive]
	public partial SaveProfile? OpenedProfile { get; set; }

	[Reactive]
	public partial string? OpenedProfilePath { get; set; }

	#endregion

	#region Child ViewModels

	[ObservableAsProperty]
	public partial InventoryViewModel? Inventory { get; }

	[ObservableAsProperty]
	public partial RandovaniaDataViewModel? RandovaniaData { get; }

	#endregion

	#region Other Properties

	[ObservableAsProperty]
	public partial bool HasRandovaniaData { get; }

	#endregion

	#region Commands

	[ReactiveCommand]
	private async Task<bool> ConfirmUnsavedChangesAsync()
	{
		if (OpenedProfilePath is null || !HasChanges)
			return true;

		return await this.dialogs.ConfirmAsync(
			"Unsaved Changes",
			"You have unsaved changes. If you open another profile, your changes will be lost.\n\nContinue?"
		);
	}

	[ReactiveCommand]
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

	[ReactiveCommand(CanExecute = nameof(IsProfileOpened))]
	private async Task SaveFileAsync(bool saveAs)
	{
		if (!IsProfileOpened)
			return;

		if (saveAs)
		{
			var newProfilePath = await this.fileBrowser.OpenFolderAsync("Choose Save Destination");

			if (newProfilePath is null)
				return;

			OpenedProfilePath = newProfilePath;
		}

		await OpenedProfile.SaveAsync(OpenedProfilePath);
	}

	[ReactiveCommand]
	private async Task CloseAsync()
	{
		if (await ConfirmUnsavedChangesAsync())
			this.windowContext.Close();
	}

	#endregion
}