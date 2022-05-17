using MercuryEngine.Data.Formats;
using StudioZDR.App.Features.SaveEditor.DataModels;
using FileNames = StudioZDR.App.Utility.Constants.FileNames.Profile;

namespace StudioZDR.App.Features.SaveEditor.Services;

public class ProfileLoader
{
	public async Task<SaveProfile> Load(string profileFolder)
	{
		var commonPath = Path.Combine(profileFolder, FileNames.Common);
		var samusPath = Path.Combine(profileFolder, FileNames.Samus);

		if (!File.Exists(commonPath))
			throw new FileNotFoundException($"A \"{FileNames.Common}\" file was not found in the provided profile folder");

		if (!File.Exists(samusPath))
			throw new FileNotFoundException($"A \"{FileNames.Samus}\" file was not found in the provided profile folder");

		await using var commonStream = File.Open(commonPath, FileMode.Open, FileAccess.Read);
		var commonBmssv = Bmssv.From(commonStream);

		await using var samusStream = File.Open(samusPath, FileMode.Open, FileAccess.Read);
		var samusBmssv = Bmssv.From(samusStream);

		return new SaveProfile(commonBmssv, samusBmssv);
	}
}