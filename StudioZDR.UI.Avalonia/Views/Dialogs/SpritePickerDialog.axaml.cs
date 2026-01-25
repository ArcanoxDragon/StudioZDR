using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using ReactiveUI;
using StudioZDR.App.ViewModels;

namespace StudioZDR.UI.Avalonia.Views.Dialogs;

public partial class SpritePickerDialog : BaseWindow<SpritePickerDialogViewModel>
{
	#region Properties

	public static readonly StyledProperty<string> MessageProperty
		= AvaloniaProperty.Register<SpritePickerDialog, string>(
			nameof(Message),
			string.Empty,
			validate: value => value != null!);

	public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
		AvaloniaProperty.Register<SpritePickerDialog, IDataTemplate?>(nameof(ItemTemplate));

	public static readonly StyledProperty<string> PositiveTextProperty
		= AvaloniaProperty.Register<SpritePickerDialog, string>(
			nameof(PositiveText),
			"_Confirm",
			validate: value => value != null!);

	public static readonly StyledProperty<string> NegativeTextProperty
		= AvaloniaProperty.Register<SpritePickerDialog, string>(
			nameof(NegativeText),
			"Ca_ncel",
			validate: value => value != null!);

	public static readonly StyledProperty<string?> AutoSelectSpriteSheetProperty
		= AvaloniaProperty.Register<SpritePickerDialog, string?>(nameof(AutoSelectSpriteSheet));

	public static readonly StyledProperty<string?> AutoSelectSpriteProperty
		= AvaloniaProperty.Register<SpritePickerDialog, string?>(nameof(AutoSelectSprite));

	public static readonly StyledProperty<bool> PositiveButtonAccentProperty
		= AvaloniaProperty.Register<SpritePickerDialog, bool>(nameof(PositiveButtonAccent), true);

	#endregion

	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	public SpritePickerDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
			Message = "Sample Dialog Text";

		this.WhenActivated(disposables => {
			var doAutoPickSprite = false;

			ViewModel?.WhenAnyValue(m => m.SpriteSheets)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(spriteSheets => {
					if (!string.IsNullOrEmpty(AutoSelectSpriteSheet))
					{
						doAutoPickSprite = true;
						ViewModel.SelectedSpriteSheet = spriteSheets.FirstOrDefault(s => string.Equals(s.Name, AutoSelectSpriteSheet, StringComparison.OrdinalIgnoreCase));
					}
				})
				.DisposeWith(disposables);

			ViewModel?.WhenAnyValue(m => m.AvailableSprites)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(sprites => {
					if (doAutoPickSprite && !string.IsNullOrEmpty(AutoSelectSprite))
					{
						doAutoPickSprite = false;
						ViewModel.SelectedSprite = sprites.FirstOrDefault(s => string.Equals(s.Name, AutoSelectSprite, StringComparison.OrdinalIgnoreCase));
					}
				})
				.DisposeWith(disposables);
		});
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

	/// <summary>
	/// Gets or sets the name of the sprite sheet to auto-select.
	/// </summary>
	public string? AutoSelectSpriteSheet
	{
		get => GetValue(AutoSelectSpriteSheetProperty);
		set => SetValue(AutoSelectSpriteSheetProperty, value);
	}

	/// <summary>
	/// Gets or sets the name of the sprite to auto-select.
	/// </summary>
	public string? AutoSelectSprite
	{
		get => GetValue(AutoSelectSpriteProperty);
		set => SetValue(AutoSelectSpriteProperty, value);
	}

	public void OnPositiveButtonClicked() => Close(ViewModel?.SelectedSprite?.FullName);
	public void OnNegativeButtonClicked() => Close(null);

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		this.SpriteSheetsList.Focus();
	}
}