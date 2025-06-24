using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData.Binding;
using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Configuration;
using StudioZDR.App.Extensions;
using StudioZDR.App.Features.GuiEditor.Configuration;
using StudioZDR.App.Framework;
using StudioZDR.App.ViewModels;
using Vector2 = System.Numerics.Vector2;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public partial class GuiEditorViewModel : ViewModelBase
{
	private readonly IWindowContext                       windowContext;
	private readonly IOptionsMonitor<ApplicationSettings> settingsMonitor;
	private readonly Stack<Bmscp>                         undoStack      = [];
	private readonly Stack<Bmscp>                         redoStack      = [];
	private readonly Subject<bool>                        canUndoSubject = new();
	private readonly Subject<bool>                        canRedoSubject = new();

	private bool ignoreNextStateChange;

	public GuiEditorViewModel(
		IWindowContext windowContext,
		ISettingsManager settingsManager,
		IOptionsMonitor<ApplicationSettings> settingsMonitor,
		ILogger<GuiEditorViewModel> logger
	)
	{
		this.windowContext = windowContext;
		this.settingsMonitor = settingsMonitor;
		this._isMouseSelectionEnabled = settingsMonitor.CurrentValue.GetFeatureSettings<GuiEditorSettings>().MouseSelectionEnabled;
		this._zoomLevel = 0;

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
			.Subscribe(file => {
				// When explicitly opening a file, we want to definitively *re-open* it even if it's the same file,
				// so we will set the property to null first before storing the chosen file.
				OpenedFilePath = null;
				OpenedFilePath = file;
			});

		this.WhenAnyValue(m => m.OpenedFilePath)
			.ObserveOn(TaskPoolScheduler)
			.InvokeCommand(LoadGuiFileCommand!);

		this.WhenAnyValue(m => m.OpenedGuiFile)
			.ObserveOn(MainThreadScheduler)
			.Subscribe(bmscp => {
				// Reset editor state when loading new file
				SelectedNodes.Clear();
				HoveredNode = null;
				ZoomLevel = 0.0;
				PanOffset = Vector2.Zero;
				LatestPristineState = bmscp;
				this.undoStack.Clear();
				this.redoStack.Clear();
				RefreshCanUndoRedo();
			});

		this.WhenAnyValue(m => m.LatestPristineState)
			.ObserveOn(MainThreadScheduler)
			.Subscribe(bmscp => {
				if (this.ignoreNextStateChange)
				{
					this.ignoreNextStateChange = false;
					return;
				}

				// Swap in new composition and dispose old
				var previousComposition = Composition;

				Composition = new DreadGuiCompositionViewModel(bmscp?.DeepClone().Root.Value);
				previousComposition?.Dispose();
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

		CanSaveFile = Observable.Return(Settings.IsOutputLocationValid)
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

		this.WhenAnyValue(m => m.ZoomLevel, level => Math.Pow(10, level))
			.ToProperty(this, m => m.ZoomFactor, out this._zoomFactorHelper);

		this.WhenAnyValue(m => m.ZoomFactor)
			.StartWith(1.0)
			.Buffer(2, 1)
			.Subscribe(buffer => {
				if (buffer is not [var previous, var current] || previous == 0.0)
					return;

				var scaleFactor = current / previous;

				PanOffset *= (float) scaleFactor;
			});

		this.WhenAnyValue(m => m.HoveredNode, n => n?.DisplayObject)
			.ToProperty(this, m => m.HoveredObject, out this._hoveredObjectHelper);

		this.WhenAnyValue(m => m.IsMouseSelectionEnabled)
			.Subscribe(enabled => {
				if (!enabled)
					// Make sure a node doesn't get stuck as hovered in case this is disabled while one *is* hovered
					HoveredNode = null;

				settingsManager.Modify(settings => {
					var guiEditorSettings = settings.GetFeatureSettings<GuiEditorSettings>();

					guiEditorSettings.MouseSelectionEnabled = enabled;
				});
			});

		this.WhenActivated(disposables => {
			if (!Settings.IsOutputLocationValid)
			{
				Dialogs.Alert(
						"Saving Disabled",
						"Warning: You have not set an Output Path in the application settings (or the chosen output path" +
						"does not exist). In order to protect your original RomFS files from accidental modification, you " +
						"will not be able to save any changes made in the editor without first setting an Output Path.")
					.Subscribe()
					.DisposeWith(disposables);
			}

			SelectedNodes
				.ToObservableChangeSet()
				.Select(_ => SelectedNodes.Select(n => n.DisplayObject).ToList())
				.ToProperty(this, m => m.SelectedObjects, out this._selectedObjectsHelper)
				.DisposeWith(disposables);

			SelectedNodes
				.ToObservableChangeSet()
				.Select(_ => SelectedNodes.Count > 0)
				.ToProperty(this, m => m.HasSelection, out this._hasSelectionHelper);

			SelectedNodes
				.ToObservableChangeSet()
				.Subscribe(_ => Composition?.InvalidateRender())
				.DisposeWith(disposables);

			Disposable.Create(() => {
				Composition?.Dispose();
				Composition = null;
			}).DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial string? OpenedFilePath { get; set; }

	[Reactive]
	public partial Bmscp? OpenedGuiFile { get; set; }

	[Reactive]
	public partial Bmscp? LatestPristineState { get; set; }

	[Reactive]
	public partial DreadGuiCompositionViewModel? Composition { get; private set; }

	[ObservableAsProperty]
	public partial bool IsLoading { get; }

	[ObservableAsProperty]
	public partial bool IsSaving { get; }

	[Reactive]
	public partial Exception? OpenFileException { get; set; }

	[Reactive]
	public partial double ZoomLevel { get; set; }

	[Reactive]
	public partial Vector2 PanOffset { get; set; }

	[ObservableAsProperty]
	public partial double ZoomFactor { get; }

	[Reactive]
	public partial GuiCompositionNodeViewModel? HoveredNode { get; set; }

	[ObservableAsProperty]
	public partial GUI__CDisplayObject? HoveredObject { get; }

	public ObservableCollection<GuiCompositionNodeViewModel> SelectedNodes { get; } = [];

	[ObservableAsProperty(ReadOnly = false)]
	public partial IReadOnlyList<GUI__CDisplayObject?>? SelectedObjects { get; }

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool HasSelection { get; }

	[Reactive]
	public partial bool IsMouseSelectionEnabled { get; set; }

	private IObservable<bool> CanSaveFile { get; }

	private ApplicationSettings Settings => this.settingsMonitor.CurrentValue;
	private IObservable<bool>   CanUndo  => this.canUndoSubject;
	private IObservable<bool>   CanRedo  => this.canRedoSubject;

	public void ToggleSelected(GuiCompositionNodeViewModel node)
	{
		// If we remove it successfully, it was in the collection before, so don't add it.
		// If we did not remove it, it was absent before, so we should add it.
		if (!SelectedNodes.Remove(node))
			SelectedNodes.Add(node);
	}

	#region Undo/Redo

	public void StageUndoOperation()
	{
		if (LatestPristineState is null || Composition is not { RootContainer: not null })
			return;

		// Clear the redo stack
		this.redoStack.Clear();

		// Store the latest "pristine" state as the most recent undo-able operation
		this.undoStack.Push(LatestPristineState);

		// Store the current composition in a new "pristine" state
		// (we need to ignore this state change for the purposes of rebuilding the hierarchy - we aren't really changing states yet)
		this.ignoreNextStateChange = true;
		LatestPristineState = CreatePristineState();

		RefreshCanUndoRedo();
	}

	[ReactiveCommand(CanExecute = nameof(CanUndo))]
	public void Undo()
	{
		if (this.undoStack.Count == 0)
			return;

		// Push the current state onto the "redo" stack
		this.redoStack.Push(CreatePristineState());

		// Pop the top undo operation into the current "pristine state"
		LatestPristineState = this.undoStack.Pop();

		RefreshCanUndoRedo();
	}

	[ReactiveCommand(CanExecute = nameof(CanRedo))]
	public void Redo()
	{
		if (this.redoStack.Count == 0)
			return;

		// Push the current state onto the "undo" stack
		this.undoStack.Push(CreatePristineState());

		// Pop the top redo operation into the current "pristine state"
		LatestPristineState = this.redoStack.Pop();

		RefreshCanUndoRedo();
	}

	private Bmscp CreatePristineState()
	{
		Bmscp wrappedCurrentState = new() {
			Root = Composition?.RootContainer,
		};

		return wrappedCurrentState.DeepClone();
	}

	private void RefreshCanUndoRedo()
	{
		this.canUndoSubject.OnNext(this.undoStack.Count > 0);
		this.canRedoSubject.OnNext(this.redoStack.Count > 0);
	}

	#endregion

	[ReactiveCommand]
	public void ResetZoomAndPan()
	{
		ZoomLevel = 0;
		PanOffset = Vector2.Zero;
	}

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

		var guiScriptsPath = Path.Join(Settings.RomFsLocation, "gui", "scripts");

		return Path.Join(guiScriptsPath, result);
	}

	[ReactiveCommand(CanExecute = nameof(CanSaveFile))]
	private async Task SaveFileAsync()
	{
		if (!Settings.IsRomFsLocationValid || !Settings.IsOutputLocationValid)
			// Just in case
			return;
		if (OpenedFilePath is not { } openedFilePath || LatestPristineState is not { } bmscp)
			return;

		await TaskPoolScheduler.Yield();

		var relativeRomFsLocation = Path.GetRelativePath(Settings.RomFsLocation, openedFilePath);
		var outputFilePath = Path.Join(Settings.OutputLocation, relativeRomFsLocation);
		var outputFileDirectory = Path.GetDirectoryName(outputFilePath)!;

		Directory.CreateDirectory(outputFileDirectory);

		await using var fileStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

		await bmscp.WriteAsync(fileStream);
	}

	private async Task<List<string>> GetAvailableGuiFilesAsync()
	{
		try
		{
			var guiScriptsPath = Path.Join(Settings.RomFsLocation, "gui", "scripts");
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
		OpenFileException = null;

		if (string.IsNullOrEmpty(guiFilePath) || !Settings.IsRomFsLocationValid)
			return null;

		await TaskPoolScheduler.Yield(cancellationToken);

		if (Settings.IsOutputLocationValid)
		{
			var relativeRomFsLocation = Path.GetRelativePath(Settings.RomFsLocation, guiFilePath);
			var matchingOutputPath = Path.Join(Settings.OutputLocation, relativeRomFsLocation);

			if (File.Exists(matchingOutputPath))
				// Read from the modified version instead!
				guiFilePath = matchingOutputPath;
		}

		await using var fileStream = File.Open(guiFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		return await Bmscp.FromAsync(fileStream, cancellationToken);
	}
}