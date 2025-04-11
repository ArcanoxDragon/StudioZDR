using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using DynamicData.Binding;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Utility;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public sealed partial class DisplayObjectPropertiesViewModel : ViewModelBase, IDisposable
{
	private const string MultipleValuesPlaceholder = "<multiple>";

	private static readonly RectangleF FullScreenBounds = new(0, 0, 1, 1);

	private readonly Subject<Unit>    changes              = new();
	private readonly SerialDisposable collectionDisposable = new();

	private volatile int refreshing;

	public DisplayObjectPropertiesViewModel()
	{
		this.WhenAnyValue(m => m.Nodes)
			.Subscribe(nodes => {
				AggregateAndRefreshValues();

				if (nodes is ObservableCollection<GuiCompositionNodeViewModel> observableCollection)
				{
					this.collectionDisposable.Disposable = observableCollection
						.ToObservableChangeSet()
						.Subscribe(_ => AggregateAndRefreshValues());
				}
				else
				{
					this.collectionDisposable.Disposable = null;
				}
			});

		this.WhenAnyValue(m => m.Id)
			.Subscribe(id => SetAllValues(n => n.ID, id));

		this.WhenAnyValue(m => m.IsVisible)
			.Subscribe(visible => SetAllValues(n => n.Visible, visible));

		this.WhenAnyValue(m => m.HorizontalAnchor)
			.Subscribe(SetHorizontalAnchors);

		this.WhenAnyValue(m => m.VerticalAnchor)
			.Subscribe(SetVerticalAnchors);
	}

	public IObservable<Unit> Changes => this.changes;

	[Reactive]
	public partial IList<GuiCompositionNodeViewModel>? Nodes { get; set; }

	[Reactive]
	public partial bool CanEdit { get; set; }

	#region ID

	[Reactive]
	public partial string? Id { get; set; }

	[Reactive]
	public partial string? IdWatermark { get; set; }

	#endregion

	#region Visible

	[Reactive]
	public partial bool? IsVisible { get; set; }

	#endregion

	#region Anchor

	[Reactive]
	public partial HorizontalAnchor? HorizontalAnchor { get; set; }

	[Reactive]
	public partial VerticalAnchor? VerticalAnchor { get; set; }

	#endregion

	public void Dispose()
	{
		this.changes.Dispose();
		this.collectionDisposable.Dispose();
	}

	private void SetHorizontalAnchors(HorizontalAnchor? anchor)
	{
		if (Nodes is null or [])
			return;

		foreach (var node in Nodes)
		{
			if (node.DisplayObject is not { } obj)
				continue;

			var bounds = GetDisplayObjectBounds(node, out var parentBounds);

			if (anchor == Properties.HorizontalAnchor.Right)
			{
				obj.RightX = parentBounds.Right - bounds.Right;
				obj.X = obj.LeftX = obj.CenterX = null;
			}
			else if (anchor == Properties.HorizontalAnchor.Center)
			{
				obj.CenterX = ( bounds.Left + bounds.Width / 2f ) - ( parentBounds.Left + parentBounds.Width / 2f );
				obj.X = obj.LeftX = obj.RightX = null;
			}
			else
			{
				obj.X = bounds.Left - parentBounds.Left;
				obj.LeftX = obj.CenterX = obj.RightX = null;
			}

			node.NotifyOfDisplayObjectChange();
		}

		this.changes.OnNext(Unit.Default);
	}

	private void SetVerticalAnchors(VerticalAnchor? anchor)
	{
		if (Nodes is null or [])
			return;

		foreach (var node in Nodes)
		{
			if (node.DisplayObject is not { } obj)
				continue;

			var bounds = GetDisplayObjectBounds(node, out var parentBounds);

			if (anchor == Properties.VerticalAnchor.Bottom)
			{
				obj.BottomY = parentBounds.Bottom - bounds.Bottom;
				obj.Y = obj.TopY = obj.CenterY = null;
			}
			else if (anchor == Properties.VerticalAnchor.Center)
			{
				obj.CenterY = ( bounds.Top + bounds.Height / 2f ) - ( parentBounds.Top + parentBounds.Height / 2f );
				obj.Y = obj.TopY = obj.BottomY = null;
			}
			else
			{
				obj.Y = bounds.Top - parentBounds.Top;
				obj.TopY = obj.CenterY = obj.BottomY = null;
			}

			node.NotifyOfDisplayObjectChange();
		}

		this.changes.OnNext(Unit.Default);
	}

	// TODO: Consolidate this with the similar method in DreadGuiCompositionDrawOperation somehow
	private static RectangleF GetDisplayObjectBounds(GuiCompositionNodeViewModel? node, out RectangleF parentBounds)
	{
		if (node?.DisplayObject is not { } obj)
		{
			parentBounds = FullScreenBounds;
			return FullScreenBounds;
		}

		parentBounds = GetDisplayObjectBounds(node.Parent, out _);

		var objWidth = obj.SizeX ?? 1.0f;
		var objHeight = obj.SizeY ?? 1.0f;
		var refX = parentBounds.X;
		var refY = parentBounds.Y;
		float boundsX, boundsY;

		if (obj.RightX.HasValue)
		{
			var rightRefX = refX + parentBounds.Width;
			var originX = rightRefX - obj.RightX.Value;

			boundsX = originX - objWidth;
		}
		else if (obj.CenterX.HasValue)
		{
			var centerRefX = refX + 0.5f * parentBounds.Width;
			var originX = centerRefX + obj.CenterX.Value;

			boundsX = originX - 0.5f * objWidth;
		}
		else
		{
			boundsX = refX + ( obj.LeftX ?? obj.X ?? 0.0f );
		}

		if (obj.BottomY.HasValue)
		{
			var bottomRefY = refY + parentBounds.Height;
			var originY = bottomRefY - obj.BottomY.Value;

			boundsY = originY - objHeight;
		}
		else if (obj.CenterY.HasValue)
		{
			var centerRefY = refY + 0.5f * parentBounds.Height;
			var originY = centerRefY + obj.CenterY.Value;

			boundsY = originY - 0.5f * objHeight;
		}
		else
		{
			boundsY = refY + ( obj.TopY ?? obj.Y ?? 0.0f );
		}

		return new RectangleF(boundsX, boundsY, objWidth, objHeight);
	}

	private void SetAllValues<T>(Expression<Func<GUI__CDisplayObject, T>> propertyExpression, T value)
		=> SetAllValues(propertyExpression, _ => value);

	private void SetAllValues<T>(Expression<Func<GUI__CDisplayObject, T>> propertyExpression, Func<GUI__CDisplayObject, T> getValue)
	{
		if (this.refreshing > 0) // Do NOT set properties on models while we're freshing FROM the models!
			return;
		if (Nodes is null)
			return;

		var property = ReflectionUtility.GetProperty(propertyExpression);
		var didChange = false;

		foreach (var node in Nodes)
		{
			if (node.DisplayObject is not { } obj)
				continue;

			var currentValue = property.GetValue(obj);
			var newValue = getValue(obj);

			if (!Equals(currentValue, newValue))
			{
				property.SetValue(obj, newValue);
				node.NotifyOfDisplayObjectChange(property.Name);
				didChange = true;
			}
		}

		if (didChange)
			this.changes.OnNext(Unit.Default);
	}

	private void AggregateAndRefreshValues()
	{
		try
		{
			if (Interlocked.Increment(ref this.refreshing) != 1)
				return;

			using var _ = DelayChangeNotifications();

			CanEdit = Nodes?.Count > 0;
			Id = null;
			IdWatermark = null;
			IsVisible = true;
			HorizontalAnchor = null;
			VerticalAnchor = null;

			if (Nodes == null)
				return;

			for (var i = 0; i < Nodes.Count; i++)
			{
				var obj = Nodes[i].DisplayObject;

				if (i == 0)
				{
					// Derive current values from first object

					Id = obj?.ID;
					IsVisible = obj?.Visible;
					HorizontalAnchor = obj?.GetHorizontalAnchor();
					VerticalAnchor = obj?.GetVerticalAnchor();
				}
				else
				{
					// If any object has a value that differs, null that property out

					if (obj?.ID != Id)
					{
						Id = null;
						IdWatermark = MultipleValuesPlaceholder;
					}

					if (obj?.Visible != IsVisible)
						IsVisible = null;
					if (obj?.GetHorizontalAnchor() != HorizontalAnchor)
						HorizontalAnchor = null;
					if (obj?.GetVerticalAnchor() != VerticalAnchor)
						VerticalAnchor = null;
				}
			}
		}
		finally
		{
			Interlocked.Decrement(ref this.refreshing);
		}
	}
}