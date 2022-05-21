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

	private void SetDataModelValue<TValue>(Expression<Func<PlayerInventory, TValue>> expression, TValue value, [CallerMemberName] string? propertyName = default)
	{
		var property = ExpressionUtility.GetProperty(expression);
		var propertyValue = (TValue?) property.GetValue(PlayerInventory);

		this.RaiseAndSetIfChanged(ref propertyValue, value, propertyName);

		property.SetValue(PlayerInventory, propertyValue);
	}
}