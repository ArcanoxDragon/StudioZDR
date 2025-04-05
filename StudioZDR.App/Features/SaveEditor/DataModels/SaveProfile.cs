using MercuryEngine.Data.Formats;
using StudioZDR.App.Utility;

namespace StudioZDR.App.Features.SaveEditor.DataModels;

public class SaveProfile
{
	public static async Task<SaveProfile> LoadFromAsync(string profileFolder, CancellationToken cancellationToken = default)
	{
		var commonPath = Path.Combine(profileFolder, ZdrConstants.FileNames.Profile.Common);
		var samusPath = Path.Combine(profileFolder, ZdrConstants.FileNames.Profile.Samus);

		if (!File.Exists(commonPath))
			throw new FileNotFoundException($"A \"{ZdrConstants.FileNames.Profile.Common}\" file was not found in the provided profile folder");

		if (!File.Exists(samusPath))
			throw new FileNotFoundException($"A \"{ZdrConstants.FileNames.Profile.Samus}\" file was not found in the provided profile folder");

		await using var commonStream = File.Open(commonPath, FileMode.Open, FileAccess.Read);
		var commonBmssv = await Bmssv.FromAsync(commonStream, cancellationToken);

		await using var samusStream = File.Open(samusPath, FileMode.Open, FileAccess.Read);
		var samusBmssv = await Bmssv.FromAsync(samusStream, cancellationToken);

		return new SaveProfile(commonBmssv, samusBmssv);
	}

	private readonly Bmssv common;
	private readonly Bmssv samus;

	private SaveProfile(Bmssv commonBmssv, Bmssv samusBmssv)
	{
		this.common = commonBmssv;
		this.samus = samusBmssv;

		Inventory = new PlayerInventory(commonBmssv);
		RandovaniaData = new RandovaniaData(commonBmssv);
	}

	public PlayerInventory Inventory      { get; }
	public RandovaniaData  RandovaniaData { get; }

	public bool HasRandovaniaData => RandovaniaData.RandoGameInitialized;

	public async Task SaveAsync(string profileFolder, CancellationToken cancellationToken = default)
	{
		var commonPath = Path.Combine(profileFolder, ZdrConstants.FileNames.Profile.Common);
		var samusPath = Path.Combine(profileFolder, ZdrConstants.FileNames.Profile.Samus);

		await using var commonStream = File.Open(commonPath, FileMode.Create, FileAccess.Write);

		await this.common.WriteAsync(commonStream, cancellationToken);

		await using var samusStream = File.Open(samusPath, FileMode.Create, FileAccess.Write);

		await this.samus.WriteAsync(samusStream, cancellationToken);
	}
}