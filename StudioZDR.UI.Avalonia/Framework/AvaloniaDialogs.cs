using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using StudioZDR.App.Framework;
using StudioZDR.UI.Avalonia.Views.Dialogs;

namespace StudioZDR.UI.Avalonia.Framework;

public class AvaloniaDialogs(Func<WindowContext?> windowContextAccessor) : IDialogs
{
	private readonly Func<WindowContext?> windowContextAccessor = windowContextAccessor;

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

	public IObservable<Unit> Alert(string title, string message, string? buttonText = null)
		=> Observable.FromAsync(() => AlertAsync(title, message, buttonText));

	public async Task AlertAsync(string title, string message, string? buttonText = null)
	{
		await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new AlertDialog { Title = title, Message = message };

			if (buttonText != null)
				dialog.ButtonText = buttonText;

			await dialog.ShowDialog(ParentWindow);
		});
	}

	public IObservable<bool> Confirm(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
		=> Observable.FromAsync(() => ConfirmAsync(title, message, positiveText, negativeText, positiveButtonAccent));

	public async Task<bool> ConfirmAsync(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	{
		return await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new ConfirmDialog { Title = title, Message = message, PositiveButtonAccent = positiveButtonAccent };

			if (positiveText != null)
				dialog.PositiveText = positiveText;
			if (negativeText != null)
				dialog.NegativeText = negativeText;

			return await dialog.ShowDialog<bool>(ParentWindow);
		});
	}
}