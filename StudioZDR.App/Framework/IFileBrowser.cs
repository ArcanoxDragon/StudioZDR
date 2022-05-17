namespace StudioZDR.App.Framework;

public record FileDialogFilter(string Name, params string[] Extensions);

public interface IFileBrowser
{
	Task<string?> OpenFileAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null);
	Task<string?> OpenFolderAsync(string? title = null);
}