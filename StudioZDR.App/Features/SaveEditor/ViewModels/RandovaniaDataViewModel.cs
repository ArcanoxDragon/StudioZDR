using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public class RandovaniaDataViewModel : ViewModelWithDataModel<RandovaniaData>
{
	private string seedHash;
	private string worldId;

	public RandovaniaDataViewModel(RandovaniaData randovaniaData)
	{
		RandovaniaData = randovaniaData;

		if (RandovaniaData.RandoSeedHash is { Length: > 8 })
		{
			this.seedHash = RandovaniaData.RandoSeedHash[..8].ToUpper();
			this.worldId = RandovaniaData.RandoSeedHash[8..];
		}
		else
		{
			this.seedHash = RandovaniaData.RandoSeedHash.ToUpper();
			this.worldId = string.Empty;
		}
	}

	protected override RandovaniaData DataModel => RandovaniaData;

	private RandovaniaData RandovaniaData { get; }

	public bool RandoGameInitialized
	{
		get => RandovaniaData.RandoGameInitialized;
		set => SetDataModelValue(m => m.RandoGameInitialized, value);
	}

	public string SeedHash
	{
		get => this.seedHash;
		set
		{
			this.RaiseAndSetIfChanged(ref this.seedHash, value.ToUpper());
			UpdateRawSeedHash();
		}
	}

	public string WorldId
	{
		get => this.worldId;
		set
		{
			this.RaiseAndSetIfChanged(ref this.worldId, value);
			UpdateRawSeedHash();
		}
	}

	#region DNA

	public bool MetroidDna1
	{
		get => RandovaniaData.MetroidDna1;
		set => SetDataModelValue(m => m.MetroidDna1, value);
	}

	public bool MetroidDna2
	{
		get => RandovaniaData.MetroidDna2;
		set => SetDataModelValue(m => m.MetroidDna2, value);
	}

	public bool MetroidDna3
	{
		get => RandovaniaData.MetroidDna3;
		set => SetDataModelValue(m => m.MetroidDna3, value);
	}

	public bool MetroidDna4
	{
		get => RandovaniaData.MetroidDna4;
		set => SetDataModelValue(m => m.MetroidDna4, value);
	}

	public bool MetroidDna5
	{
		get => RandovaniaData.MetroidDna5;
		set => SetDataModelValue(m => m.MetroidDna5, value);
	}

	public bool MetroidDna6
	{
		get => RandovaniaData.MetroidDna6;
		set => SetDataModelValue(m => m.MetroidDna6, value);
	}

	public bool MetroidDna7
	{
		get => RandovaniaData.MetroidDna7;
		set => SetDataModelValue(m => m.MetroidDna7, value);
	}

	public bool MetroidDna8
	{
		get => RandovaniaData.MetroidDna8;
		set => SetDataModelValue(m => m.MetroidDna8, value);
	}

	public bool MetroidDna9
	{
		get => RandovaniaData.MetroidDna9;
		set => SetDataModelValue(m => m.MetroidDna9, value);
	}

	public bool MetroidDna10
	{
		get => RandovaniaData.MetroidDna10;
		set => SetDataModelValue(m => m.MetroidDna10, value);
	}

	public bool MetroidDna11
	{
		get => RandovaniaData.MetroidDna11;
		set => SetDataModelValue(m => m.MetroidDna11, value);
	}

	public bool MetroidDna12
	{
		get => RandovaniaData.MetroidDna12;
		set => SetDataModelValue(m => m.MetroidDna12, value);
	}

	#endregion

	#region Upgrades

	public int FlashShiftUpgrades
	{
		get => RandovaniaData.FlashShiftUpgrades;
		set => SetDataModelValue(m => m.FlashShiftUpgrades, value);
	}

	public int SpeedBoosterUpgrades
	{
		get => RandovaniaData.SpeedBoosterUpgrades;
		set => SetDataModelValue(m => m.SpeedBoosterUpgrades, value);
	}

	#endregion

	private void UpdateRawSeedHash()
		=> RandovaniaData.RandoSeedHash = $"{this.seedHash}{this.worldId}";
}