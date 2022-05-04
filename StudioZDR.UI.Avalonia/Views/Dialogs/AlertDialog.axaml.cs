using Avalonia.Markup.Xaml;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Views.Dialogs
{
	public partial class AlertDialog : ReactiveWindow<ViewModelBase>
	{
		#region Properties

		public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<AlertDialog, string>(
			nameof(Message),
			string.Empty,
			validate: value => value != null!);

		public static readonly StyledProperty<string> ButtonTextProperty = AvaloniaProperty.Register<AlertDialog, string>(
			nameof(ButtonText),
			"_Ok",
			validate: value => value != null!);

		#endregion

		public AlertDialog()
		{
			InitializeComponent();

#if DEBUG
			this.AttachDevTools();
#endif
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
		/// Gets or sets the text shown on the dismiss button.
		/// </summary>
		public string ButtonText
		{
			get => GetValue(ButtonTextProperty);
			set => SetValue(ButtonTextProperty, value);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void OnButtonClicked() => Close();
	}
}