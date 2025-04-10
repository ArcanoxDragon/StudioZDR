namespace StudioZDR.App.Features;

public interface IFeature
{
	string  Name               { get; }
	string  Description        { get; }
	string  IconKey            { get; }
	Type    ViewModelType      { get; }
	int     DisplayOrder       { get; }
	bool    IsAvailable        { get; }
	string? UnavailableMessage { get; }
}