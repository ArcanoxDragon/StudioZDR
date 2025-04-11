using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace StudioZDR.UI.Avalonia.Converters;

internal class EqualsConverter : IValueConverter
{
	public static EqualsConverter Instance { get; } = new();

	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> Equals(value, parameter);

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? parameter : BindingOperations.DoNothing;
}