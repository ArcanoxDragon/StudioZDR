namespace StudioZDR.App.Framework;

public record FileDialogFilter(string Name, params string[] Extensions);

public interface IFileBrowser
{
	Task<string?> OpenFileAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null);
	Task<string?> OpenFolderAsync(string? title = null);
	Task<string?> SaveFileAsync(string? title = null, string? defaultExtension = null, IEnumerable<FileDialogFilter>? filters = null, bool showOverwritePrompt = true);
}