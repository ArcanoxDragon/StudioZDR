using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public partial class LabelPropertiesViewModel : DisplayObjectPropertiesViewModel
{
	public LabelPropertiesViewModel()
	{
		this.WhenAnyValue(m => m.LabelText)
			.Subscribe(labelText => SetAllValues((GUI__CLabel label) => label.Text, labelText));

		this.WhenAnyValue(m => m.Font)
			.Subscribe(font => SetAllValues((GUI__CLabel label) => label.Font, font));

		this.WhenAnyValue(m => m.Outline)
			.Subscribe(outline => SetAllValues((GUI__CLabel label) => label.Outline, outline));
	}

	#region LabelText

	[Reactive]
	public partial string? LabelText { get; set; }

	[Reactive]
	public partial string? LabelTextWatermark { get; set; }

	#endregion

	#region Font

	[Reactive]
	public partial string? Font { get; set; }

	[Reactive]
	public partial string? FontWatermark { get; set; }

	#endregion

	#region Font

	[Reactive]
	public partial bool? Outline { get; set; }

	#endregion

	protected override void ResetValues()
	{
		base.ResetValues();

		LabelText = null;
		LabelTextWatermark = null;
		Font = null;
		FontWatermark = null;
		Outline = false;
	}

	protected override void RefreshValuesFromObject(GUI__CDisplayObject? obj, bool firstObject)
	{
		base.RefreshValuesFromObject(obj, firstObject);

		GUI__CLabel? label = obj as GUI__CLabel;

		if (firstObject)
		{
			LabelText = label?.Text ?? string.Empty;
			Font = label?.Font ?? string.Empty;
			Outline = label?.Outline ?? false;
		}
		else
		{
			if (label?.Text != LabelText)
			{
				LabelText = null;
				LabelTextWatermark = MultipleValuesPlaceholder;
			}

			if (label?.Font != Font)
			{
				Font = null;
				FontWatermark = MultipleValuesPlaceholder;
			}

			if (label?.Outline != Outline)
				Outline = null;
		}
	}
}