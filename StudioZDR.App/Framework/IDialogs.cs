namespace StudioZDR.App.Framework;

public interface IDialogs
{
	Task<bool> ConfirmAsync(string title, string message);
}