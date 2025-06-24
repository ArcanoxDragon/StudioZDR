namespace StudioZDR.App.Framework.Dialogs;

public class ChooseSpriteOptions
{
	public string? AutoSelectSpriteSheet { get; set; }
	public string? AutoSelectSprite      { get; set; }
	public string? PositiveButtonText    { get; set; }
	public bool    PositiveButtonAccent  { get; set; } = true;
	public string? NegativeButtonText    { get; set; }
}