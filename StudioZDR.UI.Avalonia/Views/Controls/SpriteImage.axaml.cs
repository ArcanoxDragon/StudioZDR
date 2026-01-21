using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;
using StudioZDR.UI.Avalonia.Rendering;

namespace StudioZDR.UI.Avalonia.Views.Controls;

public partial class SpriteImage : ContentControl, IActivatableView
{
	public static readonly StyledProperty<string?> SpriteNameProperty
		= AvaloniaProperty.Register<SpriteImage, string?>(nameof(SpriteName));

	public static readonly StyledProperty<Rendering.SpriteImage?> ImageSourceProperty
		= AvaloniaProperty.Register<SpriteImage, Rendering.SpriteImage?>(nameof(ImageSource));

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All assemblies that are reflected are included as TrimmerRootAssembly, so all necessary type metadata will be preserved")]
	public SpriteImage()
	{
		InitializeComponent();

		this.WhenActivated(disposables => {
			var spriteSheetManager = App.Container.Resolve<SpriteSheetManager>();

			this.WhenAnyValue(m => m.SpriteName)
				.CombineLatest(spriteSheetManager.SpriteLoaded.StartWith(default(string?)))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(pair => {
					var (spriteName, _) = pair;
					var spriteBitmap = spriteName is null ? null : spriteSheetManager.GetOrQueueSprite(spriteName);

					ImageSource = spriteBitmap is null ? null : new Rendering.SpriteImage(spriteBitmap);
				})
				.DisposeWith(disposables);
		});
	}

	public string? SpriteName
	{
		get => GetValue(SpriteNameProperty);
		set => SetValue(SpriteNameProperty, value);
	}

	public Rendering.SpriteImage? ImageSource
	{
		get => GetValue(ImageSourceProperty);
		set => SetValue(ImageSourceProperty, value);
	}
}