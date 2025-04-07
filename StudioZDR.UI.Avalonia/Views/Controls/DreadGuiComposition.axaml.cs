using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MercuryEngine.Data.Types.DreadTypes;
using StudioZDR.UI.Avalonia.Rendering;
using StudioZDR.UI.Avalonia.Rendering.DreadGui;
using StudioZDR.UI.Avalonia.Utility;

namespace StudioZDR.UI.Avalonia.Views.Controls;

public partial class DreadGuiComposition : ContentControl
{
	public static readonly StyledProperty<GUI__CDisplayObjectContainer?> RootContainerProperty
		= AvaloniaProperty.Register<DreadGuiComposition, GUI__CDisplayObjectContainer?>(nameof(RootContainer));

	private SpriteSheetManager?               spriteSheetManager;
	private DreadGuiCompositionDrawOperation? drawOperation;

	public DreadGuiComposition()
	{
		InitializeComponent();
	}

	public GUI__CDisplayObjectContainer? RootContainer
	{
		get => GetValue(RootContainerProperty);
		set => SetValue(RootContainerProperty, value);
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		this.spriteSheetManager = App.Container.Resolve<SpriteSheetManager>();
		this.spriteSheetManager.SpriteLoaded += OnSpriteLoaded;
		this.drawOperation = new DreadGuiCompositionDrawOperation(this.spriteSheetManager);

#if DEBUG
		HotReloadHelper.MetadataUpdated += HandleMetadataReloaded;
#endif
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		this.drawOperation = null;

		if (this.spriteSheetManager != null)
		{
			this.spriteSheetManager.SpriteLoaded -= OnSpriteLoaded;
			this.spriteSheetManager.Dispose();
			this.spriteSheetManager = null;
			GC.Collect(); // Can probably free up quite a bit chunk of memory now
		}

#if DEBUG
		HotReloadHelper.MetadataUpdated -= HandleMetadataReloaded;
#endif
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == RootContainerProperty)
			Dispatcher.UIThread.Invoke(InvalidateVisual);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		if (this.drawOperation is null)
			return;

		this.drawOperation.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
		this.drawOperation.RootContainer = RootContainer;
		context.Custom(this.drawOperation);
	}

	private void OnSpriteLoaded(string obj)
		// Re-render when a sprite finishes loading
		=> Dispatcher.UIThread.Invoke(InvalidateVisual);

#if DEBUG
	private void HandleMetadataReloaded(Type[]? reloadedTypes)
	{
		if (reloadedTypes?.Contains(typeof(DreadGuiCompositionDrawOperation)) is true)
			Dispatcher.UIThread.Invoke(InvalidateVisual);
	}
#endif
}