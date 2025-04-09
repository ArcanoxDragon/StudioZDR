using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Subjects;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public sealed partial class GuiCompositionNodeViewModel : ViewModelBase, IDisposable
{
	private readonly Subject<Unit> displayObjectChanges = new();

	public GuiCompositionNodeViewModel()
	{
		this._name = "<no name>";
		this._typeName = "Unknown Type";
		this._isVisible = true;
		this._children = [];

		this.WhenAnyValue(m => m.DisplayObject, GetObjectName)
			.Subscribe(name => Name = name);

		this.WhenAnyValue(m => m.DisplayObject, GetObjectTypeName)
			.Subscribe(typeName => TypeName = typeName);
	}

	[Reactive]
	public partial GUI__CDisplayObject? DisplayObject { get; set; }

	public IObservable<Unit> DisplayObjectChanges => this.displayObjectChanges;

	[Reactive]
	public partial string Name { get; private set; }

	[Reactive]
	public partial string TypeName { get; private set; }

	[Reactive]
	public partial bool IsVisible { get; set; }

	[Reactive]
	public partial ObservableCollection<GuiCompositionNodeViewModel> Children { get; set; }

	[ReactiveCommand]
	public void ToggleVisible(bool withChildren)
	{
		if (withChildren)
			SetVisibleWithChildren(!IsVisible);
		else
			IsVisible = !IsVisible;
	}

	public void NotifyOfDisplayObjectChange(string? propertyName = null)
	{
		this.displayObjectChanges.OnNext(Unit.Default);

		// Make it look like the DisplayObject property changed too (for view binding purposes)
		this.RaisePropertyChanged(nameof(DisplayObject));

		if (propertyName is nameof(GUI__CDisplayObject.ID))
			Name = GetObjectName(DisplayObject);
	}

	public void Dispose()
	{
		this.displayObjectChanges.Dispose();

		foreach (var child in Children)
			child.Dispose();
	}

	private void SetVisibleWithChildren(bool visible)
	{
		IsVisible = visible;

		foreach (var child in Children)
			child.SetVisibleWithChildren(visible);
	}

	private static string GetObjectTypeName(GUI__CDisplayObject? o)
		=> o?.TypeName ?? "Unknown Type";

	private static string GetObjectName(GUI__CDisplayObject? o)
		=> o?.ID ?? "<no name>";
}