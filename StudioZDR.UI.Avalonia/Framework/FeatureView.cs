using Avalonia.Controls;

namespace StudioZDR.UI.Avalonia.Framework;

public static class FeatureView
{
	public static readonly AttachedProperty<Size> PreferredSizeProperty = AvaloniaProperty.RegisterAttached<UserControl, Size>("PreferredSize", typeof(FeatureView));

	public static void SetPreferredSize(AvaloniaObject element, Size value)
		=> element.SetValue(PreferredSizeProperty, value);

	public static Size GetPreferredSize(AvaloniaObject element)
		=> element.GetValue(PreferredSizeProperty);
}