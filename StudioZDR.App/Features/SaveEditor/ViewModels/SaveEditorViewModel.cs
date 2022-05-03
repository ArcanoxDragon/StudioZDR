using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI.Fody.Helpers;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public class SaveEditorViewModel : ViewModelBase, IActivatableViewModel
{
	private static readonly List<FileDialogFilter> FileDialogFilters = new() {
		new FileDialogFilter("BMSSV Files", "bmssv"),
	};

	private readonly IFileBrowser fileBrowser;
	private readonly IDialogs     dialogs;

	public SaveEditorViewModel(IFileBrowser fileBrowser, IDialogs dialogs)
	{
		this.fileBrowser = fileBrowser;
		this.dialogs = dialogs;

		this.WhenActivated(disposables => {
			OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync)
											 .DisposeWith(disposables);

			OpenFileCommand.ThrownExceptions.Subscribe(exception => Debug.WriteLine($"Error in OpenFile: {exception}"));
		});
	}

	public ViewModelActivator Activator { get; } = new();

	[Reactive]
	public string? OpenedFileName { get; set; }

	[Reactive]
	public ReactiveCommand<Unit, Unit>? OpenFileCommand { get; set; }

	private async Task<bool> ConfirmUnsavedChangesAsync()
	{
		if (OpenedFileName is null)
			return true;

		return await this.dialogs.ConfirmAsync(
				   "Unsaved Changes",
				   "You have unsaved changes. If you open another file, your changes will be lost. Continue?"
			   );
	}

	private async Task OpenFileAsync()
	{
		if (!await ConfirmUnsavedChangesAsync())
			return;

		var fileName = await this.fileBrowser.OpenFileAsync("Open Save File", FileDialogFilters);

		if (fileName is null)
			return;

		OpenedFileName = fileName;
	}
}