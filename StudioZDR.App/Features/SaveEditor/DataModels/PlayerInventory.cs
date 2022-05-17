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
}