using System.Collections.ObjectModel;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels;

public partial class GuiCompositionNodeViewModel : ViewModelBase
{
	public GuiCompositionNodeViewModel()
	{
		this._name = "<no name>";
		this._typeName = "Unknown Type";
		this._isVisible = true;
		this._children = [];

		this.WhenAnyValue(m => m.DisplayObject, o => o?.ID ?? "<no name>")
			.ToProperty(this, m => m.Name, out this._nameHelper);

		this.WhenAnyValue(m => m.DisplayObject, (GUI__CDisplayObject? o) => o?.TypeName ?? "Unknown Type")
			.ToProperty(this, m => m.TypeName, out this._typeNameHelper);
	}

	[Reactive]
	public partial GUI__CDisplayObject? DisplayObject { get; set; }

	[ObservableAsProperty]
	public partial string Name { get; }

	[ObservableAsProperty]
	public partial string TypeName { get; }

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

	private void SetVisibleWithChildren(bool visible)
	{
		IsVisible = visible;

		foreach (var child in Children)
			child.SetVisibleWithChildren(visible);
	}
}