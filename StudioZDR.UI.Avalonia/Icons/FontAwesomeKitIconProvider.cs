using System.Text.Json;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.Models;
using StudioZDR.UI.Avalonia.Icons.Models;

namespace StudioZDR.UI.Avalonia.Icons;

// We basically have to implement the entire FA side of the library ourselves,
// because all of its classes are internal and its "Style" enum doesn't contain
// a value for "Custom"
internal class FontAwesomeKitIconProvider : IIconProvider
{
	private readonly FontAwesomeKitUtf8JsonStreamProvider         streamProvider = new();
	private readonly Lazy<Dictionary<string, FontAwesomeKitIcon>> iconsLazy;

	public FontAwesomeKitIconProvider()
	{
		this.iconsLazy = new Lazy<Dictionary<string, FontAwesomeKitIcon>>(Parse);
	}

	public string Prefix => "fa";

	private Dictionary<string, FontAwesomeKitIcon> Icons => this.iconsLazy.Value;

	public IconModel GetIcon(string value)
	{
		if (!FontAwesomeIconKey.TryParse(value, out FontAwesomeIconKey? key))
			throw new KeyNotFoundException($"FontAwesome icon \"{value}\" not found!");

		if (!Icons.TryGetValue(key.Value, out FontAwesomeKitIcon? icon))
			throw new KeyNotFoundException($"FontAwesome icon \"{key.Value}\" not found!");

		if (!key.Style.HasValue)
			return icon.Svg.Values.First().ToIconModel();

		if (icon.Svg.TryGetValue(key.Style.Value, out KitSvg? svg))
			return svg.ToIconModel();

		throw new KeyNotFoundException(
			$"FontAwesome style \"{key.Style}\" not found for icon \"{key.Value}\""
		);
	}

	private Dictionary<string, FontAwesomeKitIcon> Parse()
	{
		using var stream = this.streamProvider.GetUtf8JsonStream();
		var result = JsonSerializer.Deserialize(
			stream,
			FontAwesomeIconsJsonContext.Default.DictionaryStringFontAwesomeKitIcon
		);

		return result ?? [];
	}
}