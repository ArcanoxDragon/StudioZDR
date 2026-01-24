using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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

	private CompositeDisposable hierarchyDisposables;

	public DreadGuiCompositionViewModel(GUI__CDisplayObjectContainer? rootContainer)
	{
		this.hierarchyDisposables = new CompositeDisposable();

		RootContainer = rootContainer;
		RebuildHierarchy();

		this.WhenAnyValue(m => m.RootContainer)
			.Subscribe(_ => RebuildHierarchy());
	}

	public event EventHandler? Disposing;

	public GUI__CDisplayObjectContainer? RootContainer { get; }

	public IObservable<Unit> RenderInvalidated => this.renderInvalidated;

	[Reactive]
	public partial GuiCompositionNodeViewModel Hierarchy { get; private set; }

	[Reactive]
	public partial ObservableCollection<GuiCompositionNodeViewModel> HierarchyRootCollection { get; private set; }

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

	[MemberNotNull(nameof(Hierarchy))]
	[MemberNotNull(nameof(HierarchyRootCollection))]
	public void RebuildHierarchy()
	{
		var prevHierarchyDisposables = Interlocked.Exchange(ref this.hierarchyDisposables, new CompositeDisposable());

		prevHierarchyDisposables.Dispose();

		using var _ = DelayChangeNotifications(); // Replace the two following properties simultaneously

		Hierarchy = BuildHierarchy(RootContainer, this.hierarchyDisposables).DisposeWith(this.hierarchyDisposables);
		HierarchyRootCollection = [Hierarchy];
	}

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