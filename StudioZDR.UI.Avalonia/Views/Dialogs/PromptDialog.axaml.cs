using Avalonia.Controls;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Views.Dialogs
{
	public partial class PromptDialog : ReactiveWindow<ViewModelBase>
	{
		#region Properties

		public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<PromptDialog, string>(
			nameof(Message),
			string.Empty,
			validate: value => value != null!);

		public static readonly StyledProperty<string> InputTextProperty = AvaloniaProperty.Register<PromptDialog, string>(
			nameof(InputText),
			string.Empty,
			validate: value => value != null!
		);

		public static readonly StyledProperty<string> InputWatermarkProperty = AvaloniaProperty.Register<PromptDialog, string>(
			nameof(InputWatermark),
			string.Empty,
			validate: value => value != null!
		);

		public static readonly StyledProperty<string> PositiveTextProperty = AvaloniaProperty.Register<PromptDialog, string>(
			nameof(PositiveText),
			"_OK",
			validate: value => value != null!);

		public static readonly StyledProperty<string> NegativeTextProperty = AvaloniaProperty.Register<PromptDialog, string>(
			nameof(NegativeText),
			"_Cancel",
			validate: value => value != null!);

		public static readonly StyledProperty<bool> PositiveButtonAccentProperty = AvaloniaProperty.Register<PromptDialog, bool>(
			nameof(PositiveButtonAccent),
			true);

		#endregion

		public PromptDialog()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				Message = "My dialog message";
				InputWatermark = "My watermark";
			}
		}

		/// <summary>
		/// Gets or sets the message that is shown in the body of the dialog.
		/// </summary>
		public string Message
		{
			get => GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		/// <summary>
		/// Gets or sets the value of the prompt text box.
		/// </summary>
		public string InputText
		{
			get => GetValue(InputTextProperty);
			set => SetValue(InputTextProperty, value);
		}

		/// <summary>
		/// Gets or sets the watermark text of the prompt text box.
		/// </summary>
		public string InputWatermark
		{
			get => GetValue(InputWatermarkProperty);
			set => SetValue(InputWatermarkProperty, value);
		}

		/// <summary>
		/// Gets or sets the text shown on the "positive" or "confirm" button.
		/// </summary>
		public string PositiveText
		{
			get => GetValue(PositiveTextProperty);
			set => SetValue(PositiveTextProperty, value);
		}

		/// <summary>
		/// Gets or sets the text shown on the "negative" or "cancel" button.
		/// </summary>
		public string NegativeText
		{
			get => GetValue(NegativeTextProperty);
			set => SetValue(NegativeTextProperty, value);
		}

		/// <summary>
		/// Gets or sets whether or not the "positive" button uses the accent color.
		/// </summary>
		public bool PositiveButtonAccent
		{
			get => GetValue(PositiveButtonAccentProperty);
			set => SetValue(PositiveButtonAccentProperty, value);
		}

		public void OnPositiveButtonClicked() => Close(InputText);
		public void OnNegativeButtonClicked() => Close(null);

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);

			this.InputTextBox.Focus();
		}
	}
}