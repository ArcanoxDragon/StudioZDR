using System.Globalization;
using System.Numerics;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MercuryEngine.Data.Core.Extensions;

namespace StudioZDR.UI.Avalonia.Converters;

internal class VectorToColorConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (!targetType.IsTypeOrNullable(typeof(Color)))
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

		if (value is null)
			return null;

		var r = 1f;
		var g = 1f;
		var b = 1f;
		var a = 1f;

		if (value is Vector4 v4)
		{
			r = v4.X;
			g = v4.Y;
			b = v4.Z;
			a = v4.W;
		}
		else if (value is Vector3 v3)
		{
			r = v3.X;
			g = v3.Y;
			b = v3.Z;
		}
		else
		{
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		var byteR = (byte) Math.Clamp((int) ( r * 255 ), 0, 255);
		var byteG = (byte) Math.Clamp((int) ( g * 255 ), 0, 255);
		var byteB = (byte) Math.Clamp((int) ( b * 255 ), 0, 255);
		var byteA = (byte) Math.Clamp((int) ( a * 255 ), 0, 255);

		return new Color(byteA, byteR, byteG, byteB);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (!targetType.IsTypeOrNullable(typeof(Vector3)) && !targetType.IsTypeOrNullable(typeof(Vector4)))
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

		if (value is null)
			return null;

		if (value is not Color color)
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

		var r = color.R / 255f;
		var g = color.G / 255f;
		var b = color.B / 255f;

		if (targetType.IsTypeOrNullable(typeof(Vector3)))
			return new Vector3(r, g, b);

		// Must be Vector4 instead
		var a = color.A / 255f;

		return new Vector4(r, g, b, a);
	}
}