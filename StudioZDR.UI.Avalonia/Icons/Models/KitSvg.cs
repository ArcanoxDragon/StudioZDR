using Projektanker.Icons.Avalonia.Models;

namespace StudioZDR.UI.Avalonia.Icons.Models;

internal class KitSvg
{
	public string?            Path    { get; set; }
	public IReadOnlyList<int> ViewBox { get; set; } = [];

	public IconModel ToIconModel()
	{
		var viewBox = new ViewBoxModel(ViewBox[0], ViewBox[1], ViewBox[2], ViewBox[3]);
		var path = new PathModel(Path);
		return new IconModel(viewBox, path);
	}
}