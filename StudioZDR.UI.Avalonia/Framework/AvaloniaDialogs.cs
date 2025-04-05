using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using StudioZDR.App.Framework;
using StudioZDR.UI.Avalonia.Views.Dialogs;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaDialogs : IDialogs
{
	private readonly Func<WindowContext?> windowContextAccessor;

	public AvaloniaDialogs(Func<WindowContext?> windowContextAccessor)
	{
		this.windowContextAccessor = windowContextAccessor;
	}

	private Window ParentWindow
	{
		get
		{
			var parentWindow = this.windowContextAccessor()?.CurrentWindow;

			if (parentWindow is null && Application.Current is { ApplicationLifetime: IClassicDesktopStyleApplicationLifetime desktopLifetime })
				parentWindow = desktopLifetime.MainWindow;

			if (parentWindow is null)
				throw new InvalidOperationException("Could not find a parent window for which to show the dialog!");

			return parentWindow;
		}
	}

	public Task AlertAsync(string title, string message, string? buttonText = null)
	{
		var dialog = new AlertDialog { Title = title, Message = message };

		if (buttonText != null)
			dialog.ButtonText = buttonText;

		return dialog.ShowDialog(ParentWindow);
	}

	public Task<bool> ConfirmAsync(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	{
		var dialog = new ConfirmDialog { Title = title, Message = message, PositiveButtonAccent = positiveButtonAccent };

		if (positiveText != null)
			dialog.PositiveText = positiveText;
		if (negativeText != null)
			dialog.NegativeText = negativeText;

		return dialog.ShowDialog<bool>(ParentWindow);
	}
}