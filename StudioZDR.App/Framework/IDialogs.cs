using System.Reactive;

namespace StudioZDR.App.Framework;

public interface IDialogs
{
	IObservable<Unit> Alert(string title, string message, string? buttonText = null);
	Task AlertAsync(string title, string message, string? buttonText = null);

	IObservable<bool> Confirm(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true);
	Task<bool> ConfirmAsync(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true);

	IObservable<T?> Choose<T>(string title, string message, IEnumerable<T> items, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	where T : class;

	Task<T?> ChooseAsync<T>(string title, string message, IEnumerable<T> items, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	where T : class;
}