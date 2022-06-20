using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MercuryEngine.Data.Core.Utility;
using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public class InventoryViewModel : ViewModelBase, IActivatableViewModel
{
	public InventoryViewModel(PlayerInventory playerInventory)
	{
		PlayerInventory = playerInventory;
	}

	public ViewModelActivator Activator { get; } = new();

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

	private void SetDataModelValue<TValue>(Expression<Func<PlayerInventory, TValue>> expression, TValue value, [CallerMemberName] string? propertyName = default)
	{
		var property = ExpressionUtility.GetProperty(expression);
		var propertyValue = (TValue?) property.GetValue(PlayerInventory);

		this.RaiseAndSetIfChanged(ref propertyValue, value, propertyName);

		property.SetValue(PlayerInventory, propertyValue);
	}
}