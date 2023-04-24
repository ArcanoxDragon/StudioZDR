using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using StudioZDR.App.Framework;
using FileDialogFilter = StudioZDR.App.Framework.FileDialogFilter;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaFileBrowser : IFileBrowser
{
	private readonly WindowContext windowContext;

	public AvaloniaFileBrowser(WindowContext windowContext)
	{
		this.windowContext = windowContext;
	}

	private IStorageProvider StorageProvider => this.windowContext.CurrentWindow.StorageProvider;

	public async Task<string?> OpenFileAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null)
	{
		var options = new FilePickerOpenOptions { Title = title };

		if (filters != null)
			options.FileTypeFilter = filters.Select(ToAvaloniaFilter).ToList();

		var files = await StorageProvider.OpenFilePickerAsync(options);

		if (files is not { Count: 1 })
			return null;

		var file = files.Single();

		if (!file.TryGetUri(out var uri))
			return null;

		return uri.AbsolutePath;
	}

	public async Task<string?> OpenFolderAsync(string? title = null)
	{
		var options = new FolderPickerOpenOptions { Title = title };
		var folders = await StorageProvider.OpenFolderPickerAsync(options);
		var folder = folders.SingleOrDefault();

		if (folder == null || !folder.TryGetUri(out var uri))
			return null;

		return uri.AbsolutePath;
	}

	private static FilePickerFileType ToAvaloniaFilter(FileDialogFilter filter)
		=> new(filter.Name) { Patterns = filter.Extensions.ToList() };
}