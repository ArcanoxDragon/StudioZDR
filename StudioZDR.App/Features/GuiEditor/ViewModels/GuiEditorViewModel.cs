using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MercuryEngine.Data.Formats;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Configuration;
using StudioZDR.App.Extensions;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public partial class GuiEditorViewModel : ViewModelBase
{
	private readonly IWindowContext      windowContext;
	private readonly ApplicationSettings settings;

	public GuiEditorViewModel(
		IWindowContext windowContext,
		IOptionsSnapshot<ApplicationSettings> settings,
		ILogger<GuiEditorViewModel> logger
	)
	{
		this.windowContext = windowContext;
		this.settings = settings.Value;

		OpenFileCommand
			.HandleExceptionsWith(ex => {
				logger.LogError(ex, "An exception was thrown while loading all available GUI files");
				return Dialogs.Alert(
					"Error",
					"Could not obtain the list of available GUI composition files:\n\n" +
					$"{ex.GetType().Name}: {ex.Message}",
					"Dismiss");
			})
			.WhereNotNull()
			.Subscribe(file => OpenedFilePath = file);

		this.WhenAnyValue(m => m.OpenedFilePath)
			.ObserveOn(TaskPoolScheduler)
			.InvokeCommand(LoadGuiFileCommand!);

		this.WhenAnyValue(m => m.OpenedGuiFile)
			.Select(bmscp => new DreadGuiCompositionViewModel(bmscp?.Root.Value))
			.ObserveOn(MainThreadScheduler)
			.Subscribe(model => {
				GuiCompositionViewModel?.Dispose();
				GuiCompositionViewModel = model;
			});

		LoadGuiFileCommand.IsExecuting
			.ToProperty(this, m => m.IsLoading, out this._isLoadingHelper);

		LoadGuiFileCommand
			.HandleExceptions(exceptions => exceptions.ObserveOn(MainThreadScheduler).Subscribe(ex => {
				logger.LogError(ex, "An exception was thrown while loading \"{FilePath}\"", OpenedFilePath);
				OpenFileException = ex;
				OpenedFilePath = null;
			}))
			.ObserveOn(MainThreadScheduler)
			.Subscribe(file => OpenedGuiFile = file);

		CanSaveFile = Observable.Return(this.settings.IsOutputLocationValid)
			.CombineLatest(
				this.WhenAnyValue(m => m.OpenedGuiFile),
				(outputValid, openedFile) => outputValid && openedFile != null)
			.ObserveOn(MainThreadScheduler);

		SaveFileCommand.IsExecuting
			.ToProperty(this, m => m.IsSaving, out this._isSavingHelper);

		SaveFileCommand
			.HandleExceptionsWith(ex => {
				logger.LogError(ex, "An exception was thrown while saving \"{FilePath}\"", OpenedFilePath);
				return Dialogs.Alert(
					"Error",
					"An error occurred while saving the GUI composition:\n\n" +
					$"{ex.GetType().Name}: {ex.Message}",
					"Dismiss");
			})
			.ObserveOn(MainThreadScheduler)
			.Subscribe();

		this.WhenActivated(disposables => {
			if (!this.settings.IsOutputLocationValid)
			{
				Dialogs.Alert(
						"Saving Disabled",
						"Warning: You have not set an Output Path in the application settings. In order to protect " +
						"your original RomFS files from accidental modification, you will not be able to save any " +
						"changes made in the editor without first setting an Output Path.")
					.Subscribe()
					.DisposeWith(disposables);
			}

			Disposable.Create(() => {
				GuiCompositionViewModel?.Dispose();
				GuiCompositionViewModel = null;
			}).DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial string? OpenedFilePath { get; set; }

	[Reactive]
	public partial Bmscp? OpenedGuiFile { get; set; }

	[Reactive]
	public partial DreadGuiCompositionViewModel? GuiCompositionViewModel { get; private set; }

	[ObservableAsProperty]
	public partial bool IsLoading { get; }

	[ObservableAsProperty]
	public partial bool IsSaving { get; }

	[Reactive]
	public partial Exception? OpenFileException { get; set; }

	private IObservable<bool> CanSaveFile { get; }

	[ReactiveCommand]
	private void Close()
		=> this.windowContext.Close();

	[ReactiveCommand]
	private async Task<string?> OpenFileAsync()
	{
		// TODO: Dirty check

		var guiFiles = await GetAvailableGuiFilesAsync();
		var result = await Dialogs.ChooseAsync(
			"Open GUI Composition",
			"Choose a GUI composition file to open:",
			guiFiles,
			positiveText: "Open"
		);

		if (result is null)
			return null;

		var guiScriptsPath = Path.Join(this.settings.RomFsLocation, "gui", "scripts");

		return Path.Join(guiScriptsPath, result);
	}

	[ReactiveCommand(CanExecute = nameof(CanSaveFile))]
	private async Task SaveFileAsync()
	{
		if (!this.settings.IsRomFsLocationValid || !this.settings.IsOutputLocationValid)
			// Just in case
			return;
		if (OpenedFilePath is not { } openedFilePath || OpenedGuiFile is not { } bmscp)
			return;

		await TaskPoolScheduler.Yield();

		var relativeRomFsLocation = Path.GetRelativePath(this.settings.RomFsLocation, openedFilePath);
		var outputFilePath = Path.Join(this.settings.OutputLocation, relativeRomFsLocation);
		var outputFileDirectory = Path.GetDirectoryName(outputFilePath)!;

		Directory.CreateDirectory(outputFileDirectory);

		await using var fileStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

		await bmscp.WriteAsync(fileStream);
	}

	private async Task<List<string>> GetAvailableGuiFilesAsync()
	{
		try
		{
			var guiScriptsPath = Path.Join(this.settings.RomFsLocation, "gui", "scripts");
			var availableFiles = new List<string>();

			foreach (var file in Directory.EnumerateFiles(guiScriptsPath, "*.bmscp", SearchOption.AllDirectories))
			{
				var relativePath = Path.GetRelativePath(guiScriptsPath, file);

				availableFiles.Add(relativePath);
			}

			return availableFiles;
		}
		catch (Exception ex)
		{
			await Dialogs.AlertAsync("Error", $"An error occurred while attempting to locate GUI files:\n{ex.Message}\n\nIs your RomFS folder set correctly?");
			this.windowContext.Close();
			return [];
		}
	}

	[ReactiveCommand(OutputScheduler = nameof(TaskPoolScheduler))]
	private async Task<Bmscp?> LoadGuiFileAsync(string? guiFilePath, CancellationToken cancellationToken)
	{
		OpenedGuiFile = null;
		OpenFileException = null;

		if (string.IsNullOrEmpty(guiFilePath) || !this.settings.IsRomFsLocationValid)
			return null;

		await TaskPoolScheduler.Yield(cancellationToken);

		if (this.settings.IsOutputLocationValid)
		{
			var relativeRomFsLocation = Path.GetRelativePath(this.settings.RomFsLocation, guiFilePath);
			var matchingOutputPath = Path.Join(this.settings.OutputLocation, relativeRomFsLocation);

			if (File.Exists(matchingOutputPath))
				// Read from the modified version instead!
				guiFilePath = matchingOutputPath;
		}

		await using var fileStream = File.Open(guiFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		return await Bmscp.FromAsync(fileStream, cancellationToken);
	}
}