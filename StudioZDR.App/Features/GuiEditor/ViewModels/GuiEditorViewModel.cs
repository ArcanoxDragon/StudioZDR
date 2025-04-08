using System.Reactive.Disposables;
using System.Reactive.Linq;
using MercuryEngine.Data.Formats;
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

	public GuiEditorViewModel(IWindowContext windowContext, IOptionsSnapshot<ApplicationSettings> settings)
	{
		this._availableGuiFiles = [];
		this.windowContext = windowContext;
		this.settings = settings.Value;

		this.WhenAnyValue(m => m.SelectedGuiFile)
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
			.HandleExceptions(exceptions => exceptions.ObserveOn(MainThreadScheduler).Subscribe(ex => OpenFileException = ex))
			.ObserveOn(MainThreadScheduler)
			.Subscribe(file => OpenedGuiFile = file);

		this.WhenActivated(disposables => {
			Observable.StartAsync(() => Task.Run(GetAvailableGuiFilesAsync), MainThreadScheduler)
				.Subscribe(files => AvailableGuiFiles = files)
				.DisposeWith(disposables);

			Disposable.Create(() => {
				GuiCompositionViewModel?.Dispose();
				GuiCompositionViewModel = null;
			}).DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial List<string> AvailableGuiFiles { get; set; }

	[Reactive]
	public partial string? SelectedGuiFile { get; set; }

	[Reactive]
	public partial Bmscp? OpenedGuiFile { get; set; }

	[Reactive]
	public partial DreadGuiCompositionViewModel? GuiCompositionViewModel { get; private set; }

	[ObservableAsProperty]
	public partial bool IsLoading { get; }

	[Reactive]
	public partial Exception? OpenFileException { get; set; }

	[ReactiveCommand]
	private void Close()
		=> this.windowContext.Close();

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
	private async Task<Bmscp?> LoadGuiFileAsync(string? file, CancellationToken cancellationToken)
	{
		OpenedGuiFile = null;
		OpenFileException = null;

		if (string.IsNullOrEmpty(file))
			return null;

		var guiScriptsPath = Path.Join(this.settings.RomFsLocation, "gui", "scripts");
		var guiFilePath = Path.Join(guiScriptsPath, file);
		await using var fileStream = File.Open(guiFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		return await Bmscp.FromAsync(fileStream, cancellationToken);
	}
}