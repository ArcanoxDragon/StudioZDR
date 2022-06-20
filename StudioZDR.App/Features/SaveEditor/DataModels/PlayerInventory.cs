using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Utility;
using Defaults = StudioZDR.App.Utility.Constants.BlackboardDefaults.PlayerInventory;
using Properties = StudioZDR.App.Utility.Constants.BlackboardProperties.PlayerInventory;

namespace StudioZDR.App.Features.SaveEditor.DataModels;

public class PlayerInventory
{
	public PlayerInventory(Bmssv commonBmssv)
	{
		if (!commonBmssv.Sections.TryGetValue(Constants.BlackboardSections.Common.PlayerInventory, out var inventorySection))
		{
			inventorySection = new CBlackboard__CSection();
			commonBmssv.Sections.Add(Constants.BlackboardSections.Common.PlayerInventory, inventorySection);
		}

		Inventory = inventorySection;
	}

	private CBlackboard__CSection Inventory { get; }

	#region Missiles

	public int MaxMissiles
	{
		get => Inventory.TryGetFloat(Properties.MaxMissiles, out var value) ? (int) value : Defaults.MaxMissiles;
		set => Inventory.PutValue(Properties.MaxMissiles, (float) value);
	}

	public int CurrentMissiles
	{
		get => Inventory.TryGetFloat(Properties.CurrentMissiles, out var value) ? (int) value : Defaults.CurrentMissiles;
		set => Inventory.PutValue(Properties.CurrentMissiles, (float) value);
	}

	public int MissileTanks
	{
		get => Inventory.TryGetFloat(Properties.MissileTanks, out var value) ? (int) value : Defaults.MissileTanks;
		set => Inventory.PutValue(Properties.MissileTanks, (float) value);
	}

	public int MissilePlusTanks
	{
		get => Inventory.TryGetFloat(Properties.MissilePlusTanks, out var value) ? (int) value : Defaults.MissilePlusTanks;
		set => Inventory.PutValue(Properties.MissilePlusTanks, (float) value);
	}

	#endregion

	#region Health

	public int MaxHealth
	{
		get => Inventory.TryGetFloat(Properties.MaxHealth, out var value) ? (int) value : Defaults.MaxHealth;
		set => Inventory.PutValue(Properties.MaxHealth, (float) value);
	}

	public int CurrentHealth
	{
		get => Inventory.TryGetFloat(Properties.CurrentHealth, out var value) ? (int) value : Defaults.CurrentHealth;
		set => Inventory.PutValue(Properties.CurrentHealth, (float) value);
	}

	public int EnergyTanks
	{
		get => Inventory.TryGetFloat(Properties.EnergyTanks, out var value) ? (int) value : Defaults.EnergyTanks;
		set => Inventory.PutValue(Properties.EnergyTanks, (float) value);
	}

	public int EnergyTankParts
	{
		get => Inventory.TryGetFloat(Properties.EnergyTankParts, out var value) ? (int) value : Defaults.EnergyTankParts;
		set => Inventory.PutValue(Properties.EnergyTankParts, (float) value);
	}

	#endregion

	#region Aeion

	public int MaxAeion
	{
		get => Inventory.TryGetFloat(Properties.MaxAeion, out var value) ? (int) value : Defaults.MaxAeion;
		set => Inventory.PutValue(Properties.MaxAeion, (float) value);
	}

	public int CurrentAeion
	{
		get => Inventory.TryGetFloat(Properties.CurrentAeion, out var value) ? (int) value : Defaults.CurrentAeion;
		set => Inventory.PutValue(Properties.CurrentAeion, (float) value);
	}

	#endregion
}