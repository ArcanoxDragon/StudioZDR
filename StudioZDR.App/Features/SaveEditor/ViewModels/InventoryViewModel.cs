using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public class InventoryViewModel : ViewModelWithDataModel<PlayerInventory>, IActivatableViewModel
{
	public InventoryViewModel(PlayerInventory playerInventory)
	{
		PlayerInventory = playerInventory;
	}

	public ViewModelActivator Activator { get; } = new();

	protected override PlayerInventory DataModel => PlayerInventory;

	private PlayerInventory PlayerInventory { get; }

	#region Missiles

	public int MaxMissiles
	{
		get => PlayerInventory.MaxMissiles;
		set => SetDataModelValue(m => m.MaxMissiles, value);
	}

	public int CurrentMissiles
	{
		get => PlayerInventory.CurrentMissiles;
		set => SetDataModelValue(m => m.CurrentMissiles, value);
	}

	public int MissileTanks
	{
		get => PlayerInventory.MissileTanks;
		set => SetDataModelValue(m => m.MissileTanks, value);
	}

	public int MissilePlusTanks
	{
		get => PlayerInventory.MissilePlusTanks;
		set => SetDataModelValue(m => m.MissilePlusTanks, value);
	}

	#endregion

	#region Health

	public int MaxHealth
	{
		get => PlayerInventory.MaxHealth;
		set => SetDataModelValue(m => m.MaxHealth, value);
	}

	public int CurrentHealth
	{
		get => PlayerInventory.CurrentHealth;
		set => SetDataModelValue(m => m.CurrentHealth, value);
	}

	public int EnergyTanks
	{
		get => PlayerInventory.EnergyTanks;
		set => SetDataModelValue(m => m.EnergyTanks, value);
	}

	public int EnergyTankParts
	{
		get => PlayerInventory.EnergyTankParts;
		set => SetDataModelValue(m => m.EnergyTankParts, value);
	}

	#endregion

	#region Power Bombs

	public int MaxPowerBombs
	{
		get => PlayerInventory.MaxPowerBombs;
		set => SetDataModelValue(m => m.MaxPowerBombs, value);
	}

	public int CurrentPowerBombs
	{
		get => PlayerInventory.CurrentPowerBombs;
		set => SetDataModelValue(m => m.CurrentPowerBombs, value);
	}

	public int PowerBombTanks
	{
		get => PlayerInventory.PowerBombTanks;
		set => SetDataModelValue(m => m.PowerBombTanks, value);
	}

	#endregion

	#region Aeion

	public int MaxAeion
	{
		get => PlayerInventory.MaxAeion;
		set => SetDataModelValue(m => m.MaxAeion, value);
	}

	public int CurrentAeion
	{
		get => PlayerInventory.CurrentAeion;
		set => SetDataModelValue(m => m.CurrentAeion, value);
	}

	#endregion

	#region Power-ups

	public bool ChargeBeam
	{
		get => PlayerInventory.ChargeBeam;
		set => SetDataModelValue(m => m.ChargeBeam, value);
	}

	public bool DiffusionBeam
	{
		get => PlayerInventory.DiffusionBeam;
		set => SetDataModelValue(m => m.DiffusionBeam, value);
	}

	public bool WideBeam
	{
		get => PlayerInventory.WideBeam;
		set => SetDataModelValue(m => m.WideBeam, value);
	}

	public bool PlasmaBeam
	{
		get => PlayerInventory.PlasmaBeam;
		set => SetDataModelValue(m => m.PlasmaBeam, value);
	}

	public bool WaveBeam
	{
		get => PlayerInventory.WaveBeam;
		set => SetDataModelValue(m => m.WaveBeam, value);
	}

	public bool VariaSuit
	{
		get => PlayerInventory.VariaSuit;
		set => SetDataModelValue(m => m.VariaSuit, value);
	}

	public bool GravitySuit
	{
		get => PlayerInventory.GravitySuit;
		set => SetDataModelValue(m => m.GravitySuit, value);
	}

	public bool Bomb
	{
		get => PlayerInventory.Bomb;
		set => SetDataModelValue(m => m.Bomb, value);
	}

	public bool CrossBomb
	{
		get => PlayerInventory.CrossBomb;
		set => SetDataModelValue(m => m.CrossBomb, value);
	}

	public bool PowerBomb
	{
		get => PlayerInventory.PowerBomb;
		set => SetDataModelValue(m => m.PowerBomb, value);
	}

	public bool SuperMissile
	{
		get => PlayerInventory.SuperMissile;
		set => SetDataModelValue(m => m.SuperMissile, value);
	}

	public bool IceMissile
	{
		get => PlayerInventory.IceMissile;
		set => SetDataModelValue(m => m.IceMissile, value);
	}

	public bool StormMissile
	{
		get => PlayerInventory.StormMissile;
		set => SetDataModelValue(m => m.StormMissile, value);
	}

	public bool SpinBoost
	{
		get => PlayerInventory.SpinBoost;
		set => SetDataModelValue(m => m.SpinBoost, value);
	}

	public bool SpaceJump
	{
		get => PlayerInventory.SpaceJump;
		set => SetDataModelValue(m => m.SpaceJump, value);
	}

	public bool FloorSlide
	{
		get => PlayerInventory.FloorSlide;
		set => SetDataModelValue(m => m.FloorSlide, value);
	}

	public bool SpiderMagnet
	{
		get => PlayerInventory.SpiderMagnet;
		set => SetDataModelValue(m => m.SpiderMagnet, value);
	}

	public bool MorphBall
	{
		get => PlayerInventory.MorphBall;
		set => SetDataModelValue(m => m.MorphBall, value);
	}

	public bool SpeedBooster
	{
		get => PlayerInventory.SpeedBooster;
		set => SetDataModelValue(m => m.SpeedBooster, value);
	}

	public bool GrappleBeam
	{
		get => PlayerInventory.GrappleBeam;
		set => SetDataModelValue(m => m.GrappleBeam, value);
	}

	public bool ScrewAttack
	{
		get => PlayerInventory.ScrewAttack;
		set => SetDataModelValue(m => m.ScrewAttack, value);
	}

	public bool PhantomCloak
	{
		get => PlayerInventory.PhantomCloak;
		set => SetDataModelValue(m => m.PhantomCloak, value);
	}

	public bool FlashShift
	{
		get => PlayerInventory.FlashShift;
		set => SetDataModelValue(m => m.FlashShift, value);
	}

	public bool PulseRadar
	{
		get => PlayerInventory.PulseRadar;
		set => SetDataModelValue(m => m.PulseRadar, value);
	}

	#endregion
}