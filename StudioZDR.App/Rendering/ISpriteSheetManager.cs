namespace StudioZDR.App.Rendering;

public interface ISpriteSheetManager
{
	IAsyncEnumerable<string> GetAllSpriteSheetNamesAsync(CancellationToken cancellationToken = default);
	ISpriteSheet GetSpriteSheet(string spriteSheetName);
}