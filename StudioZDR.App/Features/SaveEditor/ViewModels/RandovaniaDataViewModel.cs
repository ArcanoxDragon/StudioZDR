using StudioZDR.App.Features.SaveEditor.DataModels;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.SaveEditor.ViewModels;

public class RandovaniaDataViewModel : ViewModelWithDataModel<RandovaniaData>
{
	public RandovaniaDataViewModel(RandovaniaData randovaniaData)
	{
		RandovaniaData = randovaniaData;
	}

	public ViewModelActivator Activator { get; } = new();

	protected override RandovaniaData DataModel => RandovaniaData;

	private RandovaniaData RandovaniaData { get; }

	public bool RandoGameInitialized
	{
		get => RandovaniaData.RandoGameInitialized;
		set => SetDataModelValue(m => m.RandoGameInitialized, value);
	}

	public string RandoSeedHash
	{
		get => RandovaniaData.RandoSeedHash;
		set => SetDataModelValue(m => m.RandoSeedHash, value);
	}
}
