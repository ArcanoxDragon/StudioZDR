using Microsoft.Extensions.Primitives;

namespace StudioZDR.App.Configuration;

internal class SettingsChangeToken : IChangeToken
{
	private readonly CancellationTokenSource cts;
	private readonly CancellationChangeToken innerToken;

	public SettingsChangeToken()
	{
		this.cts = new CancellationTokenSource();
		this.innerToken = new CancellationChangeToken(this.cts.Token);
	}

	public bool HasChanged            => this.innerToken.HasChanged;
	public bool ActiveChangeCallbacks => this.innerToken.ActiveChangeCallbacks;

	public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
		=> this.innerToken.RegisterChangeCallback(callback, state);

	public void NotifyOfChange()
		=> this.cts.Cancel();
}