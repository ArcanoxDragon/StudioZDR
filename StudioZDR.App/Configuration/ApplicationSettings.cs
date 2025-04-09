using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace StudioZDR.App.Configuration;

public class ApplicationSettings
{
	public string? RomFsLocation
	{
		get;
		set
		{
			field = value;
			IsRomFsLocationValid = !string.IsNullOrEmpty(value) && Directory.Exists(value);
		}
	}

	public string? OutputLocation
	{
		get;
		set
		{
			field = value;
			IsOutputLocationValid = !string.IsNullOrEmpty(value) && Directory.Exists(value);
		}
	}

	[JsonIgnore]
	[MemberNotNullWhen(true, nameof(RomFsLocation))]
	public bool IsRomFsLocationValid { get; private set; }

	[JsonIgnore]
	[MemberNotNullWhen(true, nameof(OutputLocation))]
	public bool IsOutputLocationValid { get; private set; }

	public void CopyTo(ApplicationSettings other)
	{
		other.RomFsLocation = RomFsLocation;
		other.OutputLocation = OutputLocation;
	}
}