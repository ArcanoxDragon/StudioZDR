using Avalonia.Platform.Storage;
using StudioZDR.App.Framework;
using FileDialogFilter = StudioZDR.App.Framework.FileDialogFilter;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaFileBrowser(WindowContext windowContext) : IFileBrowser
{
	private IStorageProvider StorageProvider => windowContext.CurrentWindow.StorageProvider;

	public async Task<string?> OpenFileAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null)
	{
		var options = new FilePickerOpenOptions { Title = title };

		if (filters != null)
			options.FileTypeFilter = filters.Select(ToAvaloniaFilter).ToList();

		var files = await StorageProvider.OpenFilePickerAsync(options);

		if (files is not { Count: 1 })
			return null;

		var file = files.Single();

		return file.TryGetLocalPath();
	}

	public async Task<string?> OpenFolderAsync(string? title = null)
	{
		var options = new FolderPickerOpenOptions { Title = title };
		var folders = await StorageProvider.OpenFolderPickerAsync(options);
		var folder = folders.SingleOrDefault();

		return folder?.TryGetLocalPath();
	}

	private static FilePickerFileType ToAvaloniaFilter(FileDialogFilter filter)
		=> new(filter.Name) { Patterns = filter.Extensions.ToList() };
}