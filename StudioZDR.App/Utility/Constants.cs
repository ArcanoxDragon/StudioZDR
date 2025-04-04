namespace StudioZDR.App.Utility;

internal static class Constants
{
	public static class BlackboardDefaults
	{
		public static class PlayerInventory
		{
			#region Missiles

			public const int MaxMissiles      = 15;
			public const int CurrentMissiles  = 15;
			public const int MissileTanks     = 0;
			public const int MissilePlusTanks = 0;

			#endregion

			#region Health

			public const int MaxHealth       = 99;
			public const int CurrentHealth   = 99;
			public const int EnergyTanks     = 0;
			public const int EnergyTankParts = 0;

			#endregion

			#region Power Bombs

			public const int MaxPowerBombs     = 0;
			public const int CurrentPowerBombs = 0;
			public const int PowerBombTanks    = 0;

			#endregion

			#region Aeion

			public const int MaxAeion     = 1000;
			public const int CurrentAeion = 0;

			#endregion

			#region Power-ups

			public const bool ChargeBeam    = false;
			public const bool DiffusionBeam = false;

			public const bool WideBeam   = false;
			public const bool PlasmaBeam = false;
			public const bool WaveBeam   = false;

			public const bool VariaSuit   = false;
			public const bool GravitySuit = false;

			public const bool Bomb      = false;
			public const bool CrossBomb = false;
			public const bool PowerBomb = false;

			public const bool SuperMissile = false;
			public const bool IceMissile   = false;
			public const bool StormMissile = false;

			public const bool SpinBoost = false;
			public const bool SpaceJump = false;

			public const bool FloorSlide   = true;
			public const bool SpiderMagnet = false;
			public const bool MorphBall    = false;
			public const bool SpeedBooster = false;
			public const bool GrappleBeam  = false;
			public const bool ScrewAttack  = false;
			public const bool PhantomCloak = false;
			public const bool FlashShift   = false;
			public const bool PulseRadar   = false;

			#endregion
		}
	}

	public static class BlackboardProperties
	{
		public static class Player
		{
			#region Randovania

			public const string RandoGameInitialized = "RANDO_GAME_INITIALIZED";
			public const string SeedHash             = "THIS_RANDO_IDENTIFIER";

			#endregion
		}

		public static class PlayerInventory
		{
			#region Missiles

			public const string MaxMissiles      = "ITEM_WEAPON_MISSILE_MAX";
			public const string CurrentMissiles  = "ITEM_WEAPON_MISSILE_CURRENT";
			public const string MissileTanks     = "ITEM_MISSILE_TANKS";
			public const string MissilePlusTanks = "ITEM_MISSILE_PLUS_TANKS";

			#endregion

			#region Energy

			public const string MaxHealth       = "ITEM_MAX_LIFE";
			public const string CurrentHealth   = "ITEM_CURRENT_LIFE";
			public const string EnergyTanks     = "ITEM_ENERGY_TANKS";
			public const string EnergyTankParts = "ITEM_LIFE_SHARDS";

			#endregion

			#region Power Bombs

			public const string MaxPowerBombs     = "ITEM_WEAPON_POWER_BOMB_MAX";
			public const string CurrentPowerBombs = "ITEM_WEAPON_POWER_BOMB_CURRENT";
			public const string PowerBombTanks    = "ITEM_POWER_BOMB_TANKS";

			#endregion

			#region Aeion

			public const string MaxAeion     = "ITEM_MAX_SPECIAL_ENERGY";
			public const string CurrentAeion = "ITEM_CURRENT_SPECIAL_ENERGY";

			#endregion

			#region Power-ups

			public const string ChargeBeam    = "ITEM_WEAPON_CHARGE_BEAM";
			public const string DiffusionBeam = "ITEM_WEAPON_DIFFUSION_BEAM";

			public const string WideBeam   = "ITEM_WEAPON_WIDE_BEAM";
			public const string PlasmaBeam = "ITEM_WEAPON_PLASMA_BEAM";
			public const string WaveBeam   = "ITEM_WEAPON_WAVE_BEAM";

			public const string VariaSuit   = "ITEM_VARIA_SUIT";
			public const string GravitySuit = "ITEM_GRAVITY_SUIT";

			public const string Bomb      = "ITEM_WEAPON_BOMB";
			public const string CrossBomb = "ITEM_WEAPON_LINE_BOMB";
			public const string PowerBomb = "ITEM_WEAPON_POWER_BOMB";

			public const string SuperMissile = "ITEM_WEAPON_SUPER_MISSILE";
			public const string IceMissile   = "ITEM_WEAPON_ICE_MISSILE";
			public const string StormMissile = "ITEM_MULTILOCKON";

			public const string SpinBoost = "ITEM_DOUBLE_JUMP";
			public const string SpaceJump = "ITEM_SPACE_JUMP";

			public const string FloorSlide   = "ITEM_FLOOR_SLIDE";
			public const string SpiderMagnet = "ITEM_MAGNET_GLOVE";
			public const string MorphBall    = "ITEM_MORPH_BALL";
			public const string SpeedBooster = "ITEM_SPEED_BOOSTER";
			public const string GrappleBeam  = "ITEM_WEAPON_GRAPPLE_BEAM";
			public const string ScrewAttack  = "ITEM_SCREW_ATTACK";

			public const string PhantomCloak = "ITEM_OPTIC_CAMOUFLAGE";
			public const string FlashShift   = "ITEM_GHOST_AURA";
			public const string PulseRadar   = "ITEM_SONAR";

			#endregion

			#region Rando DNA

			public const string MetroidDnaPrefix = "ITEM_RANDO_ARTIFACT";

			public const string MetroidDna1  = $"{MetroidDnaPrefix}_1";
			public const string MetroidDna2  = $"{MetroidDnaPrefix}_2";
			public const string MetroidDna3  = $"{MetroidDnaPrefix}_3";
			public const string MetroidDna4  = $"{MetroidDnaPrefix}_4";
			public const string MetroidDna5  = $"{MetroidDnaPrefix}_5";
			public const string MetroidDna6  = $"{MetroidDnaPrefix}_6";
			public const string MetroidDna7  = $"{MetroidDnaPrefix}_7";
			public const string MetroidDna8  = $"{MetroidDnaPrefix}_8";
			public const string MetroidDna9  = $"{MetroidDnaPrefix}_9";
			public const string MetroidDna10 = $"{MetroidDnaPrefix}_10";
			public const string MetroidDna11 = $"{MetroidDnaPrefix}_11";
			public const string MetroidDna12 = $"{MetroidDnaPrefix}_12";

			#endregion

			#region Rando Upgrades

			public const string FlashShiftUpgrades   = "ITEM_UPGRADE_FLASH_SHIFT_CHAIN";
			public const string SpeedBoosterUpgrades = "ITEM_UPGRADE_SPEED_BOOST_CHARGE";

			#endregion
		}
	}

	public static class BlackboardSections
	{
		public static class Common
		{
			public const string Player          = "PLAYER";
			public const string PlayerInventory = "PLAYER_INVENTORY";
		}
	}

	public static class FileNames
	{
		public static class Profile
		{
			public const string Common    = "common.bmssv";
			public const string PkProfile = "pkprfl.bmssv";
			public const string Samus     = "samus.bmssv";
		}
	}
}