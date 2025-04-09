using System.Reactive;
using System.Reactive.Linq;

namespace StudioZDR.App.Extensions;

internal static class ObservableExtensions
{
	public static IObservable<TResult> HandleExceptions<TParam, TResult>(this IReactiveCommand<TParam, TResult> command, Func<IObservable<Exception?>, IDisposable> subscribeToExceptions)
		=> Observable.Using(
			() => subscribeToExceptions(command.ThrownExceptions),
			_ => command.Catch(Observable.Empty<TResult>())
		);

	public static IObservable<TResult> HandleExceptionsWith<TParam, TResult>(this IReactiveCommand<TParam, TResult> command, Func<Exception, IObservable<Unit>> onException)
		=> command.HandleExceptions(exceptions => exceptions.WhereNotNull().SelectMany(onException).Subscribe());
}