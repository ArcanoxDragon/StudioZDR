using Avalonia.Platform;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace StudioZDR.UI.Avalonia.Icons;

internal sealed class FontAwesomeKitUtf8JsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
{
	private static readonly Uri KitIconsUri = new("avares://StudioZDR/Icons/icons.json");

	private readonly FontAwesomeFreeUtf8JsonStreamProvider freeProvider = new();

	public Stream GetUtf8JsonStream()
	{
		if (AssetLoader.Exists(KitIconsUri))
			return AssetLoader.Open(KitIconsUri);

		return this.freeProvider.GetUtf8JsonStream();
	}
}