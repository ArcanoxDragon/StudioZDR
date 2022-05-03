using Avalonia.Controls;

namespace StudioZDR.UI.Avalonia.Extensions;

internal static class WindowExtensions
{
	public static void CenterInScreen(this Window window)
	{
		var primaryScreen = window.Screens.Primary;

		if (primaryScreen is null)
			return;

		var windowRect = PixelRect.FromRect(window.Bounds, primaryScreen.PixelDensity);
		var centeredWindowRect = primaryScreen.WorkingArea.CenterRect(windowRect);

		window.Position = centeredWindowRect.Position;
	}
}