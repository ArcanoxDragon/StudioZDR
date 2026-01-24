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

		this.WhenAnyValue(m => m.HorizontalTextAlignment)
			.Subscribe(alignment => SetAllValues((GUI__CLabel label) => label.TextAlignment, alignment));

		this.WhenAnyValue(m => m.VerticalTextAlignment)
			.Subscribe(alignment => SetAllValues((GUI__CLabel label) => label.TextVerticalAlignment, alignment));

		this.WhenAnyValue(m => m.Outline)
			.Subscribe(outline => SetAllValues((GUI__CLabel label) => label.Outline, outline));

		this.WhenAnyValue(m => m.AutoSize)
			.Subscribe(autoSize => SetAllValues((GUI__CLabel label) => label.Autosize, autoSize));
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

	#region Horizontal Text Alignment

	[Reactive]
	public partial string? HorizontalTextAlignment { get; set; }

	#endregion

	#region Vertical Text Alignment

	[Reactive]
	public partial string? VerticalTextAlignment { get; set; }

	#endregion

	#region AutoSize

	[Reactive]
	public partial bool? AutoSize { get; set; }

	#endregion

	#region Outline

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
		AutoSize = true;
		HorizontalTextAlignment = null;
		VerticalTextAlignment = null;
	}

	protected override void RefreshValuesFromObject(GUI__CDisplayObject? obj, bool firstObject)
	{
		base.RefreshValuesFromObject(obj, firstObject);

		GUI__CLabel? label = obj as GUI__CLabel;
		string labelText = label?.Text ?? string.Empty;
		string font = label?.Font ?? string.Empty;
		string? horizontalTextAlignment = label?.TextAlignment;
		string? verticalTextAlignment = label?.TextVerticalAlignment;
		bool outline = label?.Outline ?? false;
		bool autoSize = label?.Autosize ?? true;

		if (firstObject)
		{
			LabelText = labelText;
			Font = font;
			HorizontalTextAlignment = horizontalTextAlignment;
			VerticalTextAlignment = verticalTextAlignment;
			Outline = outline;
			AutoSize = autoSize;
		}
		else
		{
			if (labelText != LabelText)
			{
				LabelText = null;
				LabelTextWatermark = MultipleValuesPlaceholder;
			}

			if (font != Font)
			{
				Font = null;
				FontWatermark = MultipleValuesPlaceholder;
			}

			if (horizontalTextAlignment != HorizontalTextAlignment)
				HorizontalTextAlignment = null;

			if (verticalTextAlignment != VerticalTextAlignment)
				VerticalTextAlignment = null;

			if (outline != Outline)
				Outline = null;

			if (autoSize != AutoSize)
				AutoSize = null;
		}
	}
}