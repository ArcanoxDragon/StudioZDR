using Avalonia.Controls.Primitives;

namespace StudioZDR.UI.Avalonia.Views.Controls;

internal class RadioToggleButton : ToggleButton
{
	protected override void OnClick()
	{
		if (IsChecked is true)
			return;

		base.OnClick();
	}
}
