namespace StudioZDR.App.Framework;

public interface IDialogs
{
	Task AlertAsync(string title, string message, string? buttonText = null);
	Task<bool> ConfirmAsync(string title, string message, string? positiveText = null, string? negativeText = null);
}