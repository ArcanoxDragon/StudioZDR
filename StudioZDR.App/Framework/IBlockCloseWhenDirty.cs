namespace StudioZDR.App.Framework;

public interface IBlockCloseWhenDirty
{
	bool IsDirty { get; }

	/// <summary>
	/// Asks the user whether they want to close a window even if the window's state is considered "dirty" (i.e. unsaved).
	/// Returns <see langword="true"/> if the window should close, and <see langword="false"/> if it should not.
	/// </summary>
	Task<bool> ConfirmCloseWhenDirtyAsync();
}