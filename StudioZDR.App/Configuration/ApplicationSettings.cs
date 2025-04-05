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

	[JsonIgnore]
	public bool IsRomFsLocationValid { get; private set; }

	public void CopyTo(ApplicationSettings other)
	{
		other.RomFsLocation = RomFsLocation;
	}
}