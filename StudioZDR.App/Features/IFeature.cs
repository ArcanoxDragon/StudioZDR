namespace StudioZDR.App.Features;

public interface IFeature
{
	string  Name          { get; }
	string  Description   { get; }
	string? IconName      { get; }
	Type    ViewModelType { get; }
}