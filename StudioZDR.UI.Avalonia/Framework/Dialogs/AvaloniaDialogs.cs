using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using StudioZDR.App.Framework.Dialogs;
using StudioZDR.UI.Avalonia.Views.Dialogs;

namespace StudioZDR.UI.Avalonia.Framework.Dialogs;

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

	public IObservable<string?> Prompt(string title, string message, string? defaultValue = null, string? inputWatermark = null, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
		=> Observable.FromAsync(() => PromptAsync(title, message, defaultValue, inputWatermark, positiveText, negativeText, positiveButtonAccent));

	public async Task<string?> PromptAsync(string title, string message, string? defaultValue = null, string? inputWatermark = null, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	{
		return await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new PromptDialog { Title = title, Message = message, PositiveButtonAccent = positiveButtonAccent };

			if (defaultValue != null)
				dialog.InputText = defaultValue;
			if (inputWatermark != null)
				dialog.InputWatermark = inputWatermark;
			if (positiveText != null)
				dialog.PositiveText = positiveText;
			if (negativeText != null)
				dialog.NegativeText = negativeText;

			return await dialog.ShowDialog<string?>(ParentWindow);
		});
	}

	public IObservable<T?> Choose<T>(string title, string message, IEnumerable<T> items, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	where T : class
		=> Observable.FromAsync(() => ChooseAsync(title, message, items, positiveText, negativeText, positiveButtonAccent));

	public IObservable<T?> Choose<T>(string title, string message, IEnumerable<T> items, ExtraDialogOption[] extraOptions, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	where T : class
		=> Observable.FromAsync(() => ChooseAsync(title, message, items, positiveText, negativeText, positiveButtonAccent));

	public Task<T?> ChooseAsync<T>(string title, string message, IEnumerable<T> items, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	where T : class
		=> ChooseAsync(title, message, items, [], positiveText, negativeText, positiveButtonAccent);

	public async Task<T?> ChooseAsync<T>(string title, string message, IEnumerable<T> items, ExtraDialogOption[] extraOptions, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	where T : class
	{
		return await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new ListBoxDialog {
				Title = title,
				Message = message,
				ItemsSource = items,
				PositiveButtonAccent = positiveButtonAccent,
				ExtraOptions = extraOptions,
			};

			if (positiveText != null)
				dialog.PositiveText = positiveText;
			if (negativeText != null)
				dialog.NegativeText = negativeText;

			object? result = await dialog.ShowDialog<object?>(ParentWindow);

			return result as T;
		});
	}

	public IObservable<string?> ChooseSprite(string title, string message, ChooseSpriteOptions? options = null)
		=> Observable.FromAsync(() => ChooseSpriteAsync(title, message, options));

	public async Task<string?> ChooseSpriteAsync(string title, string message, ChooseSpriteOptions? options = null)
	{
		return await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new SpritePickerDialog {
				Title = title,
				Message = message,
				PositiveButtonAccent = options?.PositiveButtonAccent ?? true,
				AutoSelectSpriteSheet = options?.AutoSelectSpriteSheet,
				AutoSelectSprite = options?.AutoSelectSprite,
			};

			if (options?.PositiveButtonText is { } positiveText)
				dialog.PositiveText = positiveText;
			if (options?.NegativeButtonText is { } negativeText)
				dialog.NegativeText = negativeText;

			return await dialog.ShowDialog<string?>(ParentWindow);
		});
	}
}