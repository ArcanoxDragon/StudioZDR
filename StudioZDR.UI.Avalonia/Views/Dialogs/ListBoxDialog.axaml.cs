using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Views.Dialogs;

public partial class ListBoxDialog : ReactiveWindow<ListBoxDialogViewModel>
{
	#region Properties

	public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<ListBoxDialog, string>(
		nameof(Message),
		string.Empty,
		validate: value => value != null!);

	public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<ListBoxDialog, IEnumerable?>(
		nameof(ItemsSource));

	public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
		AvaloniaProperty.Register<ListBoxDialog, IDataTemplate?>(nameof(ItemTemplate));

	public static readonly StyledProperty<string> PositiveTextProperty = AvaloniaProperty.Register<ListBoxDialog, string>(
		nameof(PositiveText),
		"_Confirm",
		validate: value => value != null!);

	public static readonly StyledProperty<string> NegativeTextProperty = AvaloniaProperty.Register<ListBoxDialog, string>(
		nameof(NegativeText),
		"Ca_ncel",
		validate: value => value != null!);

	public static readonly StyledProperty<bool> PositiveButtonAccentProperty = AvaloniaProperty.Register<ListBoxDialog, bool>(
		nameof(PositiveButtonAccent),
		true);

	#endregion

	public ListBoxDialog()
	{
		InitializeComponent();

		ViewModel = new ListBoxDialogViewModel();

		if (Design.IsDesignMode)
			Message = "Sample Dialog Text";
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
	/// Gets or sets a collection that will represent the available options in the dialog.
	/// </summary>
	public IEnumerable? ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	/// <summary>
	/// Gets or sets the data template used to display the items in the control.
	/// </summary>
	public IDataTemplate? ItemTemplate
	{
		get => GetValue(ItemTemplateProperty);
		set => SetValue(ItemTemplateProperty, value);
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

	public void OnPositiveButtonClicked() => Close(ViewModel?.SelectedItem);
	public void OnNegativeButtonClicked() => Close(null);

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		this.ListBox.Focus();
	}
}