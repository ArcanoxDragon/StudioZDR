using StudioZDR.App.Rendering;

namespace StudioZDR.App.ViewModels;

public class SpriteViewModel(ISpriteSheet parentSpriteSheet, string name) : ViewModelBase
{
	public string Name     { get; } = name;
	public string FullName { get; } = $"{parentSpriteSheet.Name}/{name}";
}