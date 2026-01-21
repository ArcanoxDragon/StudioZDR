using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MercuryEngine.Data.Types.DreadTypes;
using MercuryEngine.Data.Types.Fields;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Extensions;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public sealed partial class GuiCompositionNodeViewModel : ViewModelBase, IDisposable
{
	private readonly Subject<Unit> displayObjectChanges = new();

	public GuiCompositionNodeViewModel()
		: this(null) { }

	public GuiCompositionNodeViewModel(GUI__CDisplayObject? displayObject, GuiCompositionNodeViewModel? parent = null)
	{
		DisplayObject = displayObject;
		Parent = parent;
		Name = GetObjectName(displayObject);
		this._typeName = GetObjectTypeName(displayObject);
		this._fullPath = parent is null ? Name : $"{parent.FullPath}.{Name}";
		IsVisible = true;
		Children = [];

		this.WhenAnyValue(m => m.DisplayObject, GetObjectName)
			.Subscribe(name => Name = name);

		this.WhenAnyValue(m => m.DisplayObject, GetObjectTypeName)
			.ToProperty(this, m => m.TypeName, out this._typeNameHelper, initialValue: this._typeName);

		this.WhenAnyValue(m => m.Name, m => m.Parent)
			.SelectMany(tuple => {
				// ReSharper disable once VariableHidesOuterVariable
				var (name, parent) = tuple;

				if (parent is null)
					return Observable.Return(name);

				return parent.WhenAnyValue(p => p.FullPath).Select(parentPath => $"{parentPath}.{name}");
			})
			.ToProperty(this, m => m.FullPath, out this._fullPathHelper, initialValue: this._fullPath);
	}

	[Reactive]
	public partial GUI__CDisplayObject? DisplayObject { get; set; }

	[Reactive]
	public partial GuiCompositionNodeViewModel? Parent { get; set; }

	public IObservable<Unit> DisplayObjectChanges => this.displayObjectChanges;

	[Reactive]
	public partial string Name { get; private set; }

	[ObservableAsProperty]
	public partial string FullPath { get; }

	[ObservableAsProperty]
	public partial string TypeName { get; }

	[Reactive]
	public partial bool IsVisible { get; set; }

	[Reactive]
	public partial ObservableCollection<GuiCompositionNodeViewModel> Children { get; set; }

	public GuiCompositionNodeViewModel Clone()
	{
		// Create cloned node
		var clone = new GuiCompositionNodeViewModel {
			Name = Name,
			IsVisible = IsVisible,
		};

		// Clone DisplayObject into new node
		if (DisplayObject is { } displayObject)
		{
			// We use a DreadPointer so that the clone is of the correct type (since DeepClone<T> on an object will just return that exact type)
			var objPointer = new DreadPointer<GUI__CDisplayObject>(displayObject);
			var clonedPointer = objPointer.DeepClone();

			clone.DisplayObject = clonedPointer.Value;

			if (clone.DisplayObject is GUI__CDisplayObjectContainer container)
			{
				// Clear out the container's children, as those clones are invalid. We will repopulate it below.
				container.Children.Clear();
			}
		}

		// Clone our children into new node
		foreach (var child in Children)
		{
			var clonedChild = child.Clone();

			clonedChild.Parent = clone;
			clone.Children.Add(clonedChild);

			if (clone.DisplayObject is GUI__CDisplayObjectContainer container && clonedChild.DisplayObject != null)
				container.Children.Add(clonedChild.DisplayObject);
		}

		return clone;
	}

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