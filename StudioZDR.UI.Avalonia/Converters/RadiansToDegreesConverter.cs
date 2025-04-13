using System.Globalization;
using Avalonia.Data.Converters;

namespace StudioZDR.UI.Avalonia.Converters;

internal class RadiansToDegreesConverter : IValueConverter
{
	public static RadiansToDegreesConverter Instance { get; } = new();

	public int Precision { get; set; } = 3;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		try
		{
			var radians = System.Convert.ToDouble(value);
			var degrees = radians * 180.0 / Math.PI;
			var precisionFactor = Math.Pow(10, Precision);

			return Math.Round(degrees * precisionFactor) / precisionFactor;
		}
		catch // Conversion error
		{
			return 0.0;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		try
		{
			var degrees = System.Convert.ToDouble(value);

			return degrees * Math.PI / 180.0;
		}
		catch // Conversion error
		{
			return 0.0;
		}
	}
}