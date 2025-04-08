using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.ViewModels;
using Vector2 = System.Numerics.Vector2;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public sealed partial class DreadGuiCompositionViewModel : ViewModelBase, IDisposable
{
	private readonly Subject<Unit> renderInvalidated = new();

	private CompositeDisposable? hierarchyDisposables;

	public DreadGuiCompositionViewModel(GUI__CDisplayObjectContainer? rootContainer)
	{
		RootContainer = rootContainer;
		this._hierarchy = new GuiCompositionNodeViewModel();
		this._zoomLevel = 0;

		this.WhenAnyValue(m => m.ZoomLevel, level => Math.Pow(10, level))
			.ToProperty(this, m => m.ZoomFactor, out this._zoomFactorHelper);

		this.WhenAnyValue(m => m.ZoomFactor)
			.StartWith(1.0)
			.Buffer(2, 1)
			.Subscribe(buffer => {
				if (buffer is not [var previous, var current] || previous == 0.0)
					return;

				var scaleFactor = current / previous;

				PanOffset *= (float) scaleFactor;
			});

		this.WhenAnyValue(m => m.RootContainer)
			.Subscribe(root => {
				this.hierarchyDisposables?.Dispose();

				if (root is null)
				{
					this.hierarchyDisposables = null;
					return;
				}

				this.hierarchyDisposables = new CompositeDisposable();
				Hierarchy = BuildHierarchy(root, this.hierarchyDisposables);
			});

		this.WhenAnyValue(m => m.HoveredNode, n => n?.DisplayObject)
			.ToProperty(this, m => m.HoveredObject, out this._hoveredObjectHelper);

		this.WhenAnyValue(m => m.SelectedNode, n => n?.DisplayObject)
			.ToProperty(this, m => m.SelectedObject, out this._selectedObjectHelper);
	}

	public GUI__CDisplayObjectContainer? RootContainer { get; }

	public IObservable<Unit> RenderInvalidated => this.renderInvalidated;

	[Reactive]
	public partial GuiCompositionNodeViewModel Hierarchy { get; private set; }

	[Reactive]
	public partial double ZoomLevel { get; set; }

	[Reactive]
	public partial Vector2 PanOffset { get; set; }

	[ObservableAsProperty]
	public partial double ZoomFactor { get; }

	[Reactive]
	public partial GuiCompositionNodeViewModel? HoveredNode { get; set; }

	[ObservableAsProperty]
	public partial GUI__CDisplayObject? HoveredObject { get; }

	[Reactive]
	public partial GuiCompositionNodeViewModel? SelectedNode { get; set; }

	[ObservableAsProperty]
	public partial GUI__CDisplayObject? SelectedObject { get; }

	[ReactiveCommand]
	public void ResetZoomAndPan()
	{
		ZoomLevel = 0;
		PanOffset = Vector2.Zero;
	}

	public void Dispose()
	{
		this.renderInvalidated.Dispose();
	}

	private GuiCompositionNodeViewModel BuildHierarchy(GUI__CDisplayObjectContainer? rootContainer, CompositeDisposable disposables)
	{
		if (rootContainer is null)
			return new GuiCompositionNodeViewModel();

		return CreateNode(rootContainer);

		GuiCompositionNodeViewModel CreateNode(GUI__CDisplayObject displayObject)
		{
			GuiCompositionNodeViewModel node = new() { DisplayObject = displayObject };

			if (displayObject is GUI__CDisplayObjectContainer container)
			{
				foreach (var child in container.Children.Where(c => c != null))
					node.Children.Add(CreateNode(child!));
			}

			node.WhenAnyValue(n => n.IsVisible)
				.Select(_ => Unit.Default)
				.Subscribe(this.renderInvalidated)
				.DisposeWith(disposables);

			return node;
		}
	}
}