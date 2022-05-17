using MercuryEngine.Data.Formats;

namespace StudioZDR.App.Features.SaveEditor.DataModels;

public class SaveProfile
{
	private readonly Bmssv common;
	private readonly Bmssv samus;

	public SaveProfile(Bmssv commonBmssv, Bmssv samusBmssv)
	{
		this.common = commonBmssv;
		this.samus = samusBmssv;

		Inventory = new PlayerInventory(commonBmssv);
	}

	public PlayerInventory Inventory { get; }
}