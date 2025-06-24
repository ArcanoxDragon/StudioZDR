namespace StudioZDR.App.Rendering;

public interface ISpriteSheet
{
	string Name { get; }

	IEnumerable<string> GetAllSpriteNames();
}