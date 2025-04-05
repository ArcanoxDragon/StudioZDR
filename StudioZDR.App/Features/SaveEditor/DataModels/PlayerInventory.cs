using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Utility;
using Defaults = StudioZDR.App.Utility.ZdrConstants.BlackboardDefaults.PlayerInventory;
using Properties = StudioZDR.App.Utility.ZdrConstants.BlackboardProperties.PlayerInventory;

namespace StudioZDR.App.Features.SaveEditor.DataModels;

public class PlayerInventory
{
	public PlayerInventory(Bmssv commonBmssv)
	{
		if (!commonBmssv.Sections.TryGetValue(ZdrConstants.BlackboardSections.Common.PlayerInventory, out var inventorySection))
		{
			inventorySection = new CBlackboard__CSection();
			commonBmssv.Sections.Add(ZdrConstants.BlackboardSections.Common.PlayerInventory, inventorySection);
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

	#region Power Bombs

	public int MaxPowerBombs
	{
		get => Inventory.TryGetFloat(Properties.MaxPowerBombs, out var value) ? (int) value : Defaults.MaxPowerBombs;
		set => Inventory.PutValue(Properties.MaxPowerBombs, (float) value);
	}

	public int CurrentPowerBombs
	{
		get => Inventory.TryGetFloat(Properties.CurrentPowerBombs, out var value) ? (int) value : Defaults.CurrentPowerBombs;
		set => Inventory.PutValue(Properties.CurrentPowerBombs, (float) value);
	}

	public int PowerBombTanks
	{
		get => Inventory.TryGetFloat(Properties.PowerBombTanks, out var value) ? (int) value : Defaults.PowerBombTanks;
		set => Inventory.PutValue(Properties.PowerBombTanks, (float) value);
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

	#region Power-ups

	// ReSharper disable SimplifyConditionalTernaryExpression (using boolean constants from Defaults to make it easier to change defaults if desired)

	public bool ChargeBeam
	{
		get => Inventory.TryGetFloat(Properties.ChargeBeam, out var value) ? value > 0 : Defaults.ChargeBeam;
		set => Inventory.PutValue(Properties.ChargeBeam, value ? 1.0f : 0.0f);
	}

	public bool DiffusionBeam
	{
		get => Inventory.TryGetFloat(Properties.DiffusionBeam, out var value) ? value > 0 : Defaults.DiffusionBeam;
		set => Inventory.PutValue(Properties.DiffusionBeam, value ? 1.0f : 0.0f);
	}

	public bool WideBeam
	{
		get => Inventory.TryGetFloat(Properties.WideBeam, out var value) ? value > 0 : Defaults.WideBeam;
		set => Inventory.PutValue(Properties.WideBeam, value ? 1.0f : 0.0f);
	}

	public bool PlasmaBeam
	{
		get => Inventory.TryGetFloat(Properties.PlasmaBeam, out var value) ? value > 0 : Defaults.PlasmaBeam;
		set => Inventory.PutValue(Properties.PlasmaBeam, value ? 1.0f : 0.0f);
	}

	public bool WaveBeam
	{
		get => Inventory.TryGetFloat(Properties.WaveBeam, out var value) ? value > 0 : Defaults.WaveBeam;
		set => Inventory.PutValue(Properties.WaveBeam, value ? 1.0f : 0.0f);
	}

	public bool VariaSuit
	{
		get => Inventory.TryGetFloat(Properties.VariaSuit, out var value) ? value > 0 : Defaults.VariaSuit;
		set => Inventory.PutValue(Properties.VariaSuit, value ? 1.0f : 0.0f);
	}

	public bool GravitySuit
	{
		get => Inventory.TryGetFloat(Properties.GravitySuit, out var value) ? value > 0 : Defaults.GravitySuit;
		set => Inventory.PutValue(Properties.GravitySuit, value ? 1.0f : 0.0f);
	}

	public bool Bomb
	{
		get => Inventory.TryGetFloat(Properties.Bomb, out var value) ? value > 0 : Defaults.Bomb;
		set => Inventory.PutValue(Properties.Bomb, value ? 1.0f : 0.0f);
	}

	public bool CrossBomb
	{
		get => Inventory.TryGetFloat(Properties.CrossBomb, out var value) ? value > 0 : Defaults.CrossBomb;
		set => Inventory.PutValue(Properties.CrossBomb, value ? 1.0f : 0.0f);
	}

	public bool PowerBomb
	{
		get => Inventory.TryGetFloat(Properties.PowerBomb, out var value) ? value > 0 : Defaults.PowerBomb;
		set => Inventory.PutValue(Properties.PowerBomb, value ? 1.0f : 0.0f);
	}

	public bool SuperMissile
	{
		get => Inventory.TryGetFloat(Properties.SuperMissile, out var value) ? value > 0 : Defaults.SuperMissile;
		set => Inventory.PutValue(Properties.SuperMissile, value ? 1.0f : 0.0f);
	}

	public bool IceMissile
	{
		get => Inventory.TryGetFloat(Properties.IceMissile, out var value) ? value > 0 : Defaults.IceMissile;
		set => Inventory.PutValue(Properties.IceMissile, value ? 1.0f : 0.0f);
	}

	public bool StormMissile
	{
		get => Inventory.TryGetFloat(Properties.StormMissile, out var value) ? value > 0 : Defaults.StormMissile;
		set => Inventory.PutValue(Properties.StormMissile, value ? 1.0f : 0.0f);
	}

	public bool SpinBoost
	{
		get => Inventory.TryGetFloat(Properties.SpinBoost, out var value) ? value > 0 : Defaults.SpinBoost;
		set => Inventory.PutValue(Properties.SpinBoost, value ? 1.0f : 0.0f);
	}

	public bool SpaceJump
	{
		get => Inventory.TryGetFloat(Properties.SpaceJump, out var value) ? value > 0 : Defaults.SpaceJump;
		set => Inventory.PutValue(Properties.SpaceJump, value ? 1.0f : 0.0f);
	}

	public bool FloorSlide
	{
		get => Inventory.TryGetFloat(Properties.FloorSlide, out var value) ? value > 0 : Defaults.FloorSlide;
		set => Inventory.PutValue(Properties.FloorSlide, value ? 1.0f : 0.0f);
	}

	public bool SpiderMagnet
	{
		get => Inventory.TryGetFloat(Properties.SpiderMagnet, out var value) ? value > 0 : Defaults.SpiderMagnet;
		set => Inventory.PutValue(Properties.SpiderMagnet, value ? 1.0f : 0.0f);
	}

	public bool MorphBall
	{
		get => Inventory.TryGetFloat(Properties.MorphBall, out var value) ? value > 0 : Defaults.MorphBall;
		set => Inventory.PutValue(Properties.MorphBall, value ? 1.0f : 0.0f);
	}

	public bool SpeedBooster
	{
		get => Inventory.TryGetFloat(Properties.SpeedBooster, out var value) ? value > 0 : Defaults.SpeedBooster;
		set => Inventory.PutValue(Properties.SpeedBooster, value ? 1.0f : 0.0f);
	}

	public bool GrappleBeam
	{
		get => Inventory.TryGetFloat(Properties.GrappleBeam, out var value) ? value > 0 : Defaults.GrappleBeam;
		set => Inventory.PutValue(Properties.GrappleBeam, value ? 1.0f : 0.0f);
	}

	public bool ScrewAttack
	{
		get => Inventory.TryGetFloat(Properties.ScrewAttack, out var value) ? value > 0 : Defaults.ScrewAttack;
		set => Inventory.PutValue(Properties.ScrewAttack, value ? 1.0f : 0.0f);
	}

	public bool PhantomCloak
	{
		get => Inventory.TryGetFloat(Properties.PhantomCloak, out var value) ? value > 0 : Defaults.PhantomCloak;
		set => Inventory.PutValue(Properties.PhantomCloak, value ? 1.0f : 0.0f);
	}

	public bool FlashShift
	{
		get => Inventory.TryGetFloat(Properties.FlashShift, out var value) ? value > 0 : Defaults.FlashShift;
		set => Inventory.PutValue(Properties.FlashShift, value ? 1.0f : 0.0f);
	}

	public bool PulseRadar
	{
		get => Inventory.TryGetFloat(Properties.PulseRadar, out var value) ? value > 0 : Defaults.PulseRadar;
		set => Inventory.PutValue(Properties.PulseRadar, value ? 1.0f : 0.0f);
	}

	// ReSharper restore SimplifyConditionalTernaryExpression

	#endregion
}