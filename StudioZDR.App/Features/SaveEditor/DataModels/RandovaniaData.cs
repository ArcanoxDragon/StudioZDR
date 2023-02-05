using MercuryEngine.Data.Formats;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.App.Utility;

namespace StudioZDR.App.Features.SaveEditor.DataModels;

public class RandovaniaData
{
	public RandovaniaData(Bmssv commonBmssv)
	{
		if (!commonBmssv.Sections.TryGetValue(Constants.BlackboardSections.Common.Player, out var playerSection))
		{
			playerSection = new CBlackboard__CSection();
			commonBmssv.Sections.Add(Constants.BlackboardSections.Common.Player, playerSection);
		}

		Player = playerSection;
	}

	private CBlackboard__CSection Player { get; }

	public bool RandoGameInitialized
	{
		get => Player.TryGetBoolean(Constants.BlackboardProperties.Player.RandoGameInitialized, out var value) && value;
		set => Player.PutValue(Constants.BlackboardProperties.Player.RandoGameInitialized, value);
	}

	public string RandoSeedHash
	{
		get => Player.TryGetString(Constants.BlackboardProperties.Player.SeedHash, out var value) ? value : string.Empty;
		set => Player.PutValue(Constants.BlackboardProperties.Player.SeedHash, value);
	}
}