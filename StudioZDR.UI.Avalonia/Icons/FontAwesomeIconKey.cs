using System.Diagnostics.CodeAnalysis;
using StudioZDR.UI.Avalonia.Icons.Models;

namespace StudioZDR.UI.Avalonia.Icons;

internal class FontAwesomeIconKey
{
	private const string FaKeyPrefix = "fa-";

	public string    Value { get; set; } = string.Empty;
	public KitStyle? Style { get; set; }

	public static bool TryParse(string value, [MaybeNullWhen(false)] out FontAwesomeIconKey key)
	{
		var parts = value.Split([' '], StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length == 1)
		{
			key = new FontAwesomeIconKey {
				Value = FontAwesomeIconKey.GetValue(parts[0]),
			};
			return true;
		}

		if (parts.Length == 2)
		{
			key = new FontAwesomeIconKey {
				Style = FontAwesomeIconKey.GetStyle(parts[0]),
				Value = FontAwesomeIconKey.GetValue(parts[1]),
			};

			return true;
		}

		key = null;
		return false;
	}

	private static KitStyle? GetStyle(string value)
	{
		return value.ToUpperInvariant() switch {
			"FA-SOLID" or "FAS"   => KitStyle.Solid,
			"FA-REGULAR" or "FAR" => KitStyle.Regular,
			"FA-BRANDS" or "FAB"  => KitStyle.Brands,
			"FA-KIT" or "FAK"     => KitStyle.Custom,
			_                     => null,
		};
	}

	private static string GetValue(string input)
		=> input.Length <= FaKeyPrefix.Length
			? string.Empty
			: input[FaKeyPrefix.Length..];
}