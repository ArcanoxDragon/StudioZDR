namespace StudioZDR.App.Utility;

internal static class Constants
{
	public static class BlackboardDefaults
	{
		public static class PlayerInventory
		{
			public const int MaxMissiles      = 15;
			public const int CurrentMissiles  = 15;
			public const int MissileTanks     = 0;
			public const int MissilePlusTanks = 0;
			public const int MaxHealth        = 99;
			public const int CurrentHealth    = 99;
			public const int EnergyTanks      = 0;
			public const int EnergyTankParts  = 0;
		}
	}

	public static class BlackboardProperties
	{
		public static class PlayerInventory
		{
			public const string MaxMissiles      = "ITEM_WEAPON_MISSILE_MAX";
			public const string CurrentMissiles  = "ITEM_WEAPON_MISSILE_CURRENT";
			public const string MissileTanks     = "ITEM_MISSILE_TANKS";
			public const string MissilePlusTanks = "ITEM_MISSILE_PLUS_TANKS";
			public const string MaxHealth        = "ITEM_MAX_LIFE";
			public const string CurrentHealth    = "ITEM_CURRENT_LIFE";
			public const string EnergyTanks      = "ITEM_ENERGY_TANKS";
			public const string EnergyTankParts  = "ITEM_LIFE_SHARDS";
		}
	}

	public static class BlackboardSections
	{
		public static class Common
		{
			public const string PlayerInventory = "PLAYER_INVENTORY";
		}
	}

	public static class FileNames
	{
		public static class Profile
		{
			public const string Common    = "common.bmssv";
			public const string PKProfile = "pkprfl.bmssv";
			public const string Samus     = "samus.bmssv";
		}
	}
}