using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Utility;
using Properties = StudioZDR.App.Utility.Constants.BlackboardProperties.PlayerInventory;

namespace StudioZDR.App.Features.SaveEditor.DataModels;

public class RandovaniaData
{
	public RandovaniaData(Bmssv commonBmssv)
	{
		if (!commonBmssv.Sections.TryGetValue(Constants.BlackboardSections.Common.Player, out var playerSection))
		{
			playerSection = new CBlackboard__CSection();
			commonBmssv.Sections.Add(Constants.BlackboardSections.Common.Player, playerSection);
		}

		Player = playerSection;

		if (!commonBmssv.Sections.TryGetValue(Constants.BlackboardSections.Common.PlayerInventory, out var inventorySection))
		{
			inventorySection = new CBlackboard__CSection();
			commonBmssv.Sections.Add(Constants.BlackboardSections.Common.PlayerInventory, inventorySection);
		}

		Inventory = inventorySection;
	}

	private CBlackboard__CSection Player    { get; }
	private CBlackboard__CSection Inventory { get; }

	public bool RandoGameInitialized
	{
		get => Player.TryGetBoolean(Constants.BlackboardProperties.Player.RandoGameInitialized, out var value) && value;
		set => Player.PutValue(Constants.BlackboardProperties.Player.RandoGameInitialized, value);
	}

	public string RandoSeedHash
	{
		get => Player.TryGetString(Constants.BlackboardProperties.Player.SeedHash, out var value) ? value : string.Empty;
		set => Player.PutValue(Constants.BlackboardProperties.Player.SeedHash, value);
	}

	#region DNA

	public bool MetroidDna1
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna1, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna1, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna2
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna2, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna2, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna3
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna3, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna3, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna4
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna4, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna4, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna5
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna5, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna5, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna6
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna6, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna6, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna7
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna7, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna7, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna8
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna8, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna8, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna9
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna9, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna9, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna10
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna10, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna10, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna11
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna11, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna11, value ? 1.0f : 0.0f);
	}

	public bool MetroidDna12
	{
		get => Inventory.TryGetFloat(Properties.MetroidDna12, out var value) && value > 0;
		set => Inventory.PutValue(Properties.MetroidDna12, value ? 1.0f : 0.0f);
	}

	#endregion

	#region Upgrades

	public int FlashShiftUpgrades
	{
		get => Inventory.TryGetFloat(Properties.FlashShiftUpgrades, out var value) ? (int) value : 0;
		set => Inventory.PutValue(Properties.FlashShiftUpgrades, (float) value);
	}

	public int SpeedBoosterUpgrades
	{
		get => Inventory.TryGetFloat(Properties.SpeedBoosterUpgrades, out var value) ? (int) value : 0;
		set => Inventory.PutValue(Properties.SpeedBoosterUpgrades, (float) value);
	}

	#endregion
}