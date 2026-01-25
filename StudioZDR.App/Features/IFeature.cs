using System.Diagnostics.CodeAnalysis;

namespace StudioZDR.App.Features;

public interface IFeature
{
	string  Name               { get; }
	string  Description        { get; }
	string  IconKey            { get; }
	int     DisplayOrder       { get; }
	bool    IsAvailable        { get; }
	string? UnavailableMessage { get; }

	Type ViewModelType
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		get;
	}
}