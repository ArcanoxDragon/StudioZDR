using Avalonia.Controls;
using Avalonia.Threading;
using StudioZDR.App.Framework;

namespace StudioZDR.UI.Avalonia.Framework;

public sealed record WindowContext(Window CurrentWindow) : IWindowContext
{
	public void Close()
		=> Dispatcher.UIThread.Post(() => CurrentWindow.Close());
}