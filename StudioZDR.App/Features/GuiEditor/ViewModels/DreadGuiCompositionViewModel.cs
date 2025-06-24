using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public sealed partial class DreadGuiCompositionViewModel : ViewModelBase, IDisposable
{
	private readonly Subject<Unit>        renderInvalidated = new();
	private readonly ReaderWriterLockSlim hierarchyLock     = new();

	private CompositeDisposable? hierarchyDisposables;

	public DreadGuiCompositionViewModel(GUI__CDisplayObjectContainer? rootContainer)
	{
		RootContainer = rootContainer;
		this._hierarchy = new GuiCompositionNodeViewModel();

		this.WhenAnyValue(m => m.RootContainer)
			.Subscribe(root => {
				this.hierarchyDisposables?.Dispose();

				if (root is null)
				{
					this.hierarchyDisposables = null;
					return;
				}

				this.hierarchyDisposables = new CompositeDisposable();
				Hierarchy = BuildHierarchy(root, this.hierarchyDisposables).DisposeWith(this.hierarchyDisposables);
			});
	}

	public event EventHandler? Disposing;

	public GUI__CDisplayObjectContainer? RootContainer { get; }

	public IObservable<Unit> RenderInvalidated => this.renderInvalidated;

	[Reactive]
	public partial GuiCompositionNodeViewModel Hierarchy { get; private set; }

	[MustDisposeResource]
	public IDisposable LockForReading()
	{
		this.hierarchyLock.EnterReadLock();

		return Disposable.Create(this.hierarchyLock.ExitReadLock);
	}

	[MustDisposeResource]
	public IDisposable LockForWriting()
	{
		this.hierarchyLock.EnterWriteLock();

		return Disposable.Create(this.hierarchyLock.ExitWriteLock);
	}

	public void InvalidateRender()
		=> this.renderInvalidated.OnNext(Unit.Default);

	public void Dispose()
	{
		try
		{
			Disposing?.Invoke(this, EventArgs.Empty);
		}
		finally
		{
			this.renderInvalidated.Dispose();
			this.hierarchyLock.Dispose();
			this.hierarchyDisposables?.Dispose();
		}
	}

	private GuiCompositionNodeViewModel BuildHierarchy(GUI__CDisplayObjectContainer? rootContainer, CompositeDisposable disposables)
	{
		if (rootContainer is null)
			return new GuiCompositionNodeViewModel();

		return CreateNode(rootContainer);

		GuiCompositionNodeViewModel CreateNode(GUI__CDisplayObject displayObject, GuiCompositionNodeViewModel? parent = null)
		{
			GuiCompositionNodeViewModel node = new(displayObject, parent);

			if (displayObject is GUI__CDisplayObjectContainer container)
			{
				foreach (var child in container.Children.Where(c => c != null))
					node.Children.Add(CreateNode(child!, node));
			}

			node.WhenAnyValue(n => n.IsVisible)
				.Select(_ => Unit.Default)
				.Merge(node.DisplayObjectChanges)
				.Subscribe(this.renderInvalidated)
				.DisposeWith(disposables);

			return node;
		}
	}
}