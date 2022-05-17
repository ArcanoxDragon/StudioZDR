using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using StudioZDR.App.Framework;
using AvaloniaFileDialogFilter = Avalonia.Controls.FileDialogFilter;
using FileDialogFilter = StudioZDR.App.Framework.FileDialogFilter;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaFileBrowser : IFileBrowser
{
	private readonly WindowContext windowContext;

	public AvaloniaFileBrowser(WindowContext windowContext)
	{
		this.windowContext = windowContext;
	}

	public async Task<string?> OpenFileAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null)
	{
		var dialog = new OpenFileDialog { Title = title };

		if (filters != null)
			dialog.Filters.AddRange(filters.Select(ToAvaloniaFilter));

		var result = await dialog.ShowAsync(this.windowContext.CurrentWindow);

		if (result is not { Length: 1 })
			return null;

		return result.Single();
	}

	public async Task<string?> OpenFolderAsync(string? title = null)
	{
		var dialog = new OpenFolderDialog { Title = title };

		return await dialog.ShowAsync(this.windowContext.CurrentWindow);
	}

	private static AvaloniaFileDialogFilter ToAvaloniaFilter(FileDialogFilter filter)
		=> new() { Name = filter.Name, Extensions = filter.Extensions.ToList() };
}