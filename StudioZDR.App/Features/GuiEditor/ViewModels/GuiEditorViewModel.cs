using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
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
using StudioZDR.App.Framework.Dialogs;
using StudioZDR.App.ViewModels;
using Vector2 = System.Numerics.Vector2;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public partial class GuiEditorViewModel : ViewModelBase, IBlockCloseWhenDirty, IWindowTitleProvider
{
	private readonly IWindowContext                       windowContext;
	private readonly IOptionsMonitor<ApplicationSettings> settingsMonitor;

	private readonly Stack<Bmscp> undoStack = [];
	private readonly Stack<Bmscp> redoStack = [];

	private readonly Subject<bool> hasSelectionSubject = new();
	private readonly Subject<bool> canUndoSubject      = new();
	private readonly Subject<bool> canRedoSubject      = new();

	private bool ignoreNextStateChange;
	private bool forceOpenOriginalFile;

	public GuiEditorViewModel(
		IWindowContext windowContext,
		ISettingsManager settingsManager,
		IOptionsMonitor<ApplicationSettings> settingsMonitor,
		ILogger<GuiEditorViewModel> logger
	)
	{
		this.windowContext = windowContext;
		this.settingsMonitor = settingsMonitor;
		IsMouseSelectionEnabled = settingsMonitor.CurrentValue.GetFeatureSettings<GuiEditorSettings>().MouseSelectionEnabled;
		ZoomLevel = 0;

		#region OpenFileCommand

		OpenBuiltinFileCommand
			.HandleExceptionsWith(ex => {
				logger.LogError(ex, "An exception was thrown while loading all available GUI files");
				return Dialogs.Alert(
					"Error",
					"Could not obtain the list of available GUI composition files:\n\n" +
					$"{ex.GetType().Name}: {ex.Message}",
					"Dismiss");
			})
			.WhereNotNull()
			.Subscribe(tuple => {
				var (file, ignoreModifications) = tuple;

				this.forceOpenOriginalFile = ignoreModifications;

				// When explicitly opening a file, we want to definitively *re-open* it even if it's the same file,
				// so we will set the property to null first before storing the chosen file.
				OpenedFilePath = null;
				OpenedFilePath = file;
			});

		#endregion

		#region OpenedFilePath

		this.WhenAnyValue(m => m.OpenedFilePath)
			.ObserveOn(TaskPoolScheduler)
			.InvokeCommand(LoadGuiFileCommand);

		this.WhenAnyValue(m => m.OpenedFilePath)
			.Select(Path.GetFileName)
			.CombineLatest(this.WhenAnyValue(m => m.IsDirty))
			.Select(pair => {
				var (fileName, isDirty) = pair;
				var fileNameSuffix = fileName is null ? null : $" - {fileName}";
				var dirtySuffix = isDirty ? "*" : "";

				return $"{GuiEditorFeature.FeatureName}{fileNameSuffix}{dirtySuffix}";
			})
			.ObserveOn(MainThreadScheduler)
			.ToProperty(this, m => m.WindowTitle, out this._windowTitleHelper);

		#endregion

		#region OpenedGuiFile

		this.WhenAnyValue(m => m.OpenedGuiFile)
			.ObserveOn(MainThreadScheduler)
			.Subscribe(bmscp => {
				// Reset editor state when loading new file
				SelectedNodes.Clear();
				HoveredNode = null;
				ZoomLevel = 0.0;
				PanOffset = Vector2.Zero;
				LatestPristineState = bmscp;
				IsDirty = false;
				this.undoStack.Clear();
				this.redoStack.Clear();
				RefreshCanUndoRedo();
			});

		#endregion

		#region LatestPristineState

		this.WhenAnyValue(m => m.LatestPristineState)
			.ObserveOn(MainThreadScheduler)
			.Subscribe(bmscp => {
				if (this.ignoreNextStateChange)
				{
					this.ignoreNextStateChange = false;
					return;
				}

				// Record the currently-selected nodes in a way we can use to restore them
				HashSet<string> selectedNodePaths = SelectedNodes.Select(n => n.FullPath).ToHashSet();

				// Swap in new composition and dispose old
				var previousComposition = Composition;

				Composition = new DreadGuiCompositionViewModel(bmscp?.DeepClone().Root.Value);
				previousComposition?.Dispose();

				// Try and restore the selection
				SelectedNodes.Clear();
				Visit(Composition.Hierarchy);

				void Visit(GuiCompositionNodeViewModel node)
				{
					if (selectedNodePaths.Contains(node.FullPath))
						SelectedNodes.Add(node);

					foreach (var child in node.Children)
						Visit(child);
				}
			});

		#endregion

		#region LoadGuiFileCommand

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

		#endregion

		#region SaveFileCommand

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

		#endregion

		#region Zoom

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

		#endregion

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
				.Subscribe(this.hasSelectionSubject)
				.DisposeWith(disposables);

			SelectedNodes
				.ToObservableChangeSet()
				.Subscribe(_ => Composition?.InvalidateRender())
				.DisposeWith(disposables);

			Disposable.Create(() => {
				Composition?.Dispose();
				Composition = null;
			}).DisposeWith(disposables);

			disposables.Add(this.hasSelectionSubject);
			disposables.Add(this.canUndoSubject);
			disposables.Add(this.canRedoSubject);
		});
	}

	[ObservableAsProperty]
	public partial string? WindowTitle { get; }

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
	public partial bool IsDirty { get; private set; }

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

	[Reactive]
	public partial bool IsMouseSelectionEnabled { get; set; }

	private IObservable<bool> CanSaveFile { get; }

	private ApplicationSettings Settings     => this.settingsMonitor.CurrentValue;
	private IObservable<bool>   HasSelection => this.hasSelectionSubject;
	private IObservable<bool>   CanUndo      => this.canUndoSubject;
	private IObservable<bool>   CanRedo      => this.canRedoSubject;

	#region Selection

	[ReactiveCommand]
	public void SelectAll()
	{
		if (Composition is not { Hierarchy: var rootNode })
			return;

		Visit(rootNode);
		return;

		void Visit(GuiCompositionNodeViewModel node)
		{
			if (!SelectedNodes.Contains(node))
				SelectedNodes.Add(node);

			foreach (var child in node.Children)
				Visit(child);
		}
	}

	public void ToggleSelected(GuiCompositionNodeViewModel node)
	{
		// If we remove it successfully, it was in the collection before, so don't add it.
		// If we did not remove it, it was absent before, so we should add it.
		if (!SelectedNodes.Remove(node))
			SelectedNodes.Add(node);
	}

	[ReactiveCommand(CanExecute = nameof(HasSelection))]
	public void CloneSelectedObjects()
	{
		if (Composition is not { } composition)
			return;

		using var _ = composition.LockForWriting();
		var clonedObjects = new HashSet<GUI__CDisplayObject>(ReferenceEqualityComparer.Instance);

		foreach (GuiCompositionNodeViewModel node in GetTopmostSelectedNodes())
		{
			if (node.DisplayObject is not { } displayObject)
				// Invalid node!
				continue;
			if (node.Parent is not { DisplayObject: GUI__CDisplayObjectContainer parentContainer })
				// Node must belong to another container node
				continue;

			// Clone node by serializing and deserializing it again.
			// We use DreadPointer<T> instead of DeepClone so that we get the correct derived display object type.
			var objPointer = new DreadPointer<GUI__CDisplayObject>(displayObject);
			var clonedPointer = objPointer.DeepClone();
			var clonedObject = clonedPointer.Value!;

			// Add the cloned object to the parent container (and our "cloned objects" set)
			clonedObjects.Add(clonedObject);
			parentContainer.Children.Add(clonedObject);
		}

		// Stage the delete as an undo operation
		StageUndoOperation();

		// Rebuild the hierarchy to ensure all subscriptions are up-to-date
		composition.RebuildHierarchy();

		// Select the newly-cloned objects
		var nodesToSelect = composition.Hierarchy.EnumerateSelfAndChildren()
			.Where(n => n.DisplayObject != null && clonedObjects.Contains(n.DisplayObject));

		SelectedNodes.Clear();
		SelectedNodes.AddRange(nodesToSelect);
	}

	[ReactiveCommand(CanExecute = nameof(HasSelection))]
	public void DeleteSelectedObjects()
	{
		if (Composition is not { } composition)
			return;

		using var _ = composition.LockForWriting();

		foreach (GuiCompositionNodeViewModel node in GetTopmostSelectedNodes())
		{
			if (node.Parent is not { } parent)
				// This would mean we're deleting the root - can't do that!
				continue;

			// Remove the node from its parent, and remove the node's display object from the parent's display object container
			parent.Children.Remove(node);

			if (parent.DisplayObject is GUI__CDisplayObjectContainer parentContainer && node.DisplayObject is { } displayObject)
				parentContainer.Children.Remove(displayObject);
		}

		// Stage the delete as an undo operation
		StageUndoOperation();
	}

	/// <summary>
	/// Gets a set of all selected nodes who do not have any selected ancestors.
	/// In other words, if a node is selected <i>and</i> some of its descendants are selected, only the top
	/// node will be returned.
	/// </summary>
	public IEnumerable<GuiCompositionNodeViewModel> GetTopmostSelectedNodes()
	{
		HashSet<GuiCompositionNodeViewModel> selectedNodes = [..SelectedNodes];

		foreach (GuiCompositionNodeViewModel node in SelectedNodes)
		{
			if (IsAncestorSelected(node))
				// If one of the node's ancestors is already selected, don't include the node itself
				selectedNodes.Remove(node);
		}

		return selectedNodes;

		bool IsAncestorSelected(GuiCompositionNodeViewModel node)
		{
			if (node.Parent is not { } parent)
				return false;
			if (selectedNodes.Contains(parent))
				return true;

			return IsAncestorSelected(parent);
		}
	}

	#endregion

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

		// Any staged change automatically makes the state "dirty"
		IsDirty = true;
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

	#region Adding Items

	[ReactiveCommand]
	public void AddDisplayObjectContainer()
		=> AddObject(() => new GUI__CDisplayObjectContainer {
			ID = "NewContainer",
			SizeX = 0.5f,
			SizeY = 0.5f,
		});

	[ReactiveCommand]
	public void AddLabel()
		=> AddObject(() => new GUI__CLabel {
			ID = "NewLabel",
			Text = "New Label",
			Font = "digital_small",
			TextAlignment = "Centered",
			TextVerticalAlignment = "Centered",
			SizeX = 0.5f,
			SizeY = 0.1f,
		});

	[ReactiveCommand]
	public async Task AddSpriteAsync()
	{
		var sprite = await Dialogs.ChooseSpriteAsync(
			"Choose New Sprite",
			"Choose a sprite for the new CSprite object");

		if (sprite is null)
			return;

		AddObject(() => new GUI__CSprite {
			ID = "NewSprite",
			SpriteSheetItem = sprite,
			BlendMode = "AlphaBlend",
			USelMode = "Scale",
			VSelMode = "Scale",
			// TODO: Dynamic sprite size? (need abstraction over SpriteSheetManager)
			SizeX = 0.25f,
			SizeY = 0.25f,
		});
	}

	private void AddObject(Func<GUI__CDisplayObject> objectFactory)
	{
		if (Composition is not { } composition)
			return;
		if (GetTargetContainerForAdd() is not { } targetContainer)
			return;

		// Create and add new object
		var newObject = objectFactory();

		// Set common properties for all object types
		newObject.Enabled = true;
		newObject.Visible = true;
		newObject.CenterX = 0.0f;
		newObject.CenterY = 0.0f;
		newObject.ColorR = 1.0f;
		newObject.ColorG = 1.0f;
		newObject.ColorB = 1.0f;
		newObject.ColorA = 1.0f;

		targetContainer.Children.Add(newObject);

		// Stage the new object as an undo operation
		StageUndoOperation();

		// Rebuild hierarchy to update node structure and subscriptions
		composition.RebuildHierarchy();

		// Select new node
		var nodeToSelect = composition.Hierarchy.EnumerateSelfAndChildren().SingleOrDefault(n => ReferenceEquals(n.DisplayObject, newObject));

		SelectedNodes.Clear();

		if (nodeToSelect != null)
			SelectedNodes.Add(nodeToSelect);
	}

	private GUI__CDisplayObjectContainer? GetTargetContainerForAdd()
	{
		if (SelectedNodes is [{ DisplayObject: GUI__CDisplayObjectContainer singleSelectedContainer }])
			return singleSelectedContainer;

		// Add to root if a single container is not selected
		return Composition?.Hierarchy.DisplayObject as GUI__CDisplayObjectContainer;
	}

	#endregion

	[ReactiveCommand]
	public void ResetZoomAndPan()
	{
		ZoomLevel = 0;
		PanOffset = Vector2.Zero;
	}

	[ReactiveCommand]
	private async Task CloseAsync()
	{
		if (IsDirty && !await ConfirmCloseWhenDirtyAsync())
			return;

		this.windowContext.Close();
	}

	[ReactiveCommand]
	private async Task<(string? FilePath, bool IgnoreModifications)> OpenBuiltinFileAsync()
	{
		if (!await ConfirmUnsavedChangesAsync())
			return ( null, false );

		var guiFiles = await GetAvailableGuiFilesAsync();
		var ignoreModificationsOption = new ExtraDialogCheckboxOption(
			"Ignore modifications",
			"Load the original version of the composition, even if there is a modified version in the mod output folder"
		);
		var result = await Dialogs.ChooseAsync(
			"Open GUI Composition",
			"Choose a GUI composition file to open:",
			guiFiles,
			[ignoreModificationsOption],
			positiveText: "Open"
		);

		if (result is null)
			return ( null, false );

		var guiScriptsPath = Path.Join(Settings.RomFsLocation, "gui", "scripts");
		var finalFilePath = Path.Join(guiScriptsPath, result);
		var ignoreModifications = ignoreModificationsOption.Value;

		return ( finalFilePath, ignoreModifications );
	}

	private async Task<bool> ConfirmUnsavedChangesAsync()
	{
		if (OpenedGuiFile is null || !IsDirty)
			return true;

		return await Dialogs.ConfirmAsync(
			"Unsaved Changes",
			"You have unsaved changes. If you open another composition, your changes will be lost.\n\nDo you still want to proceed?"
		);
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
		IsDirty = false;
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

		if (Settings.IsOutputLocationValid && !this.forceOpenOriginalFile)
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

	#region IBlockCloseWhenDirty

	public async Task<bool> ConfirmCloseWhenDirtyAsync()
	{
		if (OpenedGuiFile is null)
			return true;

		return await Dialogs.ConfirmAsync(
			"Unsaved Changes",
			"You have unsaved changes. If you close the editor now, your changes will be lost.\n\nDo you still want to close?"
		);
	}

	#endregion
}