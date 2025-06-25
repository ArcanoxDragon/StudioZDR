using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using DynamicData.Binding;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Utility;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public partial class DisplayObjectPropertiesViewModel : ViewModelBase, IDisposable
{
	protected const string MultipleValuesPlaceholder = "<multiple>";

	private static readonly RectangleF FullScreenBounds = new(0, 0, 1, 1);

	private readonly Subject<Unit>    changes              = new();
	private readonly SerialDisposable collectionDisposable = new();
	private readonly SerialDisposable nodesDisposable      = new();

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
						.Subscribe(_ => {
							AggregateAndRefreshValues();
							this.nodesDisposable.Disposable = SubscribeAllNodes();
						});

					this.nodesDisposable.Disposable = SubscribeAllNodes();
				}
				else
				{
					this.collectionDisposable.Disposable = null;
					this.nodesDisposable.Disposable = null;
				}

				IDisposable? SubscribeAllNodes()
					=> nodes
						.Select(n => n.DisplayObjectChanges)
						.Merge()
						.Subscribe(_ => AggregateAndRefreshValues());
			});

		this.WhenAnyValue(m => m.Id)
			.Subscribe(id => SetAllValues(n => n.ID, id));

		this.WhenAnyValue(m => m.IsVisible)
			.Subscribe(visible => SetAllValues(n => n.Visible, visible));

		this.WhenAnyValue(m => m.HorizontalAnchor)
			.Subscribe(SetHorizontalAnchors);

		this.WhenAnyValue(m => m.VerticalAnchor)
			.Subscribe(SetVerticalAnchors);

		this.WhenAnyValue(m => m.PositionX, m => m.PositionY)
			.Subscribe(SetPosition);

		this.WhenAnyValue(m => m.SizeX)
			.Subscribe(x => SetAllValues(n => n.SizeX, x));

		this.WhenAnyValue(m => m.SizeY)
			.Subscribe(y => SetAllValues(n => n.SizeY, y));

		this.WhenAnyValue(m => m.Angle)
			.Subscribe(angle => SetAllValues(n => n.Angle, angle));
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

	#region Position

	[Reactive]
	public partial float? PositionX { get; set; }

	[Reactive]
	public partial string? PositionXWatermark { get; set; }

	[Reactive]
	public partial float? PositionY { get; set; }

	[Reactive]
	public partial string? PositionYWatermark { get; set; }

	#endregion

	#region Size

	[Reactive]
	public partial float? SizeX { get; set; }

	[Reactive]
	public partial string? SizeXWatermark { get; set; }

	[Reactive]
	public partial float? SizeY { get; set; }

	[Reactive]
	public partial string? SizeYWatermark { get; set; }

	#endregion

	#region Angle

	[Reactive]
	public partial float? Angle { get; set; }

	[Reactive]
	public partial string? AngleWatermark { get; set; }

	#endregion

	protected bool IsRefreshing => this.refreshing > 0;

	#region IDisposable

	private bool disposed;

	~DisplayObjectPropertiesViewModel()
		=> Dispose(false);

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (this.disposed)
			return;

		this.disposed = true;

		if (disposing)
			DisposeSelf();
	}

	protected virtual void DisposeSelf()
	{
		this.changes.Dispose();
		this.collectionDisposable.Dispose();
	}

	#endregion

	/// <summary>
	/// Suppresses any updates of display object values from the ViewModel until the
	/// returned <see cref="IDisposable"/> is disposed.
	/// </summary>
	public IDisposable SuppressValueUpdates()
	{
		Interlocked.Increment(ref this.refreshing);

		return Disposable.Create(() => Interlocked.Decrement(ref this.refreshing));
	}

	private void SetHorizontalAnchors(HorizontalAnchor? anchor)
	{
		if (IsRefreshing) // Do NOT set properties on models while we're freshing FROM the models!
			return;
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
		AggregateAndRefreshValues();
	}

	private void SetVerticalAnchors(VerticalAnchor? anchor)
	{
		if (IsRefreshing) // Do NOT set properties on models while we're freshing FROM the models!
			return;
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
		AggregateAndRefreshValues();
	}

	private void SetPosition((float? X, float? Y) position)
	{
		if (IsRefreshing) // Do NOT set properties on models while we're freshing FROM the models!
			return;
		if (Nodes is null or [])
			return;

		var (x, y) = position;
		var didChange = false;
		using var delayedNotifications = new CompositeDisposable();

		foreach (var node in Nodes)
		{
			if (node.DisplayObject is not { } obj)
				continue;

			node.DelayChangeNotifications().DisposeWith(delayedNotifications);

			obj.X = obj.LeftX = obj.CenterX = obj.RightX = null;
			obj.Y = obj.TopY = obj.CenterY = obj.BottomY = null;

			Expression<Func<GUI__CDisplayObject, float?>> xPropertyExpression = obj.GetHorizontalAnchor() switch {
				Properties.HorizontalAnchor.Right  => o => o.RightX,
				Properties.HorizontalAnchor.Center => o => o.CenterX,
				_                                  => o => o.X,
			};
			Expression<Func<GUI__CDisplayObject, float?>> yPropertyExpression = obj.GetVerticalAnchor() switch {
				Properties.VerticalAnchor.Bottom => o => o.BottomY,
				Properties.VerticalAnchor.Center => o => o.CenterY,
				_                                => o => o.Y,
			};

			didChange |= SetValue(node, xPropertyExpression, x);
			didChange |= SetValue(node, yPropertyExpression, y);
		}

		if (didChange)
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

	protected void SetAllValues<T>(Expression<Func<GUI__CDisplayObject, T>> propertyExpression, T value)
		=> SetAllValues(propertyExpression, _ => value);

	protected void SetAllValues<T>(Expression<Func<GUI__CDisplayObject, T>> propertyExpression, Func<GUI__CDisplayObject, T> getValue)
		=> SetAllValues<GUI__CDisplayObject, T>(propertyExpression, getValue);

	protected void SetAllValues<TDisplayObject, T>(Expression<Func<TDisplayObject, T>> propertyExpression, T value)
	where TDisplayObject : GUI__CDisplayObject
		=> SetAllValues(propertyExpression, _ => value);

	protected void SetAllValues<TDisplayObject, T>(Expression<Func<TDisplayObject, T>> propertyExpression, Func<TDisplayObject, T> getValue)
	where TDisplayObject : GUI__CDisplayObject
	{
		if (IsRefreshing) // Do NOT set properties on models while we're freshing FROM the models!
			return;
		if (Nodes is null)
			return;

		var property = ReflectionUtility.GetProperty(propertyExpression);
		var didChange = false;

		foreach (var node in Nodes)
			didChange |= SetValue(node, property, getValue);

		if (didChange)
		{
			this.changes.OnNext(Unit.Default);
			AggregateAndRefreshValues();
		}
	}

	protected bool SetValue<T>(GuiCompositionNodeViewModel node, Expression<Func<GUI__CDisplayObject, T>> propertyExpression, T value)
		=> SetValue<GUI__CDisplayObject, T>(node, propertyExpression, value);

	protected bool SetValue<T>(GuiCompositionNodeViewModel node, PropertyInfo property, Func<GUI__CDisplayObject, T> getValue)
		=> SetValue<GUI__CDisplayObject, T>(node, property, getValue);

	protected bool SetValue<TDisplayObject, T>(GuiCompositionNodeViewModel node, Expression<Func<TDisplayObject, T>> propertyExpression, T value)
	where TDisplayObject : GUI__CDisplayObject
	{
		var property = ReflectionUtility.GetProperty(propertyExpression);

		return SetValue<TDisplayObject, T>(node, property, _ => value);
	}

	protected bool SetValue<TDisplayObject, T>(GuiCompositionNodeViewModel node, PropertyInfo property, Func<TDisplayObject, T> getValue)
	where TDisplayObject : GUI__CDisplayObject
	{
		if (IsRefreshing) // Do NOT set properties on models while we're freshing FROM the models!
			return false;
		if (node.DisplayObject is not TDisplayObject obj)
			return false;

		var currentValue = property.GetValue(obj);
		var newValue = getValue(obj);

		if (Equals(currentValue, newValue))
			return false;

		property.SetValue(obj, newValue);
		node.NotifyOfDisplayObjectChange(property.Name);
		return true;
	}

	protected void AggregateAndRefreshValues()
	{
		try
		{
			if (Interlocked.Increment(ref this.refreshing) != 1)
				return;

			using var _ = DelayChangeNotifications();

			CanEdit = Nodes?.Count > 0;
			ResetValues();

			if (Nodes == null)
				return;

			for (var i = 0; i < Nodes.Count; i++)
			{
				var obj = Nodes[i].DisplayObject;

				RefreshValuesFromObject(obj, forceRefresh: i == 0);
			}
		}
		finally
		{
			Interlocked.Decrement(ref this.refreshing);
		}
	}

	protected virtual void ResetValues()
	{
		Id = null;
		IdWatermark = null;
		IsVisible = true;
		HorizontalAnchor = null;
		VerticalAnchor = null;
		PositionX = null;
		PositionXWatermark = null;
		PositionY = null;
		PositionYWatermark = null;
		SizeX = null;
		SizeXWatermark = null;
		SizeY = null;
		SizeYWatermark = null;
		Angle = null;
		AngleWatermark = null;
	}

	protected virtual void RefreshValuesFromObject(GUI__CDisplayObject? obj, bool forceRefresh)
	{
		var objHorizontalAnchor = obj?.GetHorizontalAnchor();
		var objVerticalAnchor = obj?.GetVerticalAnchor();
		var objPositionX = objHorizontalAnchor switch {
			Properties.HorizontalAnchor.Right  => obj?.RightX,
			Properties.HorizontalAnchor.Center => obj?.CenterX,
			_                                  => obj?.LeftX ?? obj?.X,
		};
		var objPositionY = objVerticalAnchor switch {
			Properties.VerticalAnchor.Bottom => obj?.BottomY,
			Properties.VerticalAnchor.Center => obj?.CenterY,
			_                                => obj?.TopY ?? obj?.Y,
		};

		if (forceRefresh)
		{
			// Derive current values from first object

			Id = obj?.ID;
			IsVisible = obj?.Visible;
			HorizontalAnchor = objHorizontalAnchor;
			VerticalAnchor = objVerticalAnchor;
			PositionX = objPositionX;
			PositionY = objPositionY;
			SizeX = obj?.SizeX;
			SizeY = obj?.SizeY;
			Angle = obj?.Angle;
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
			if (objHorizontalAnchor != HorizontalAnchor)
				HorizontalAnchor = null;
			if (objVerticalAnchor != VerticalAnchor)
				VerticalAnchor = null;

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (objPositionX != PositionX)
			{
				PositionX = null;
				PositionXWatermark = MultipleValuesPlaceholder;
			}

			if (objPositionY != PositionY)
			{
				PositionY = null;
				PositionYWatermark = MultipleValuesPlaceholder;
			}

			if (obj?.SizeX != SizeX)
			{
				SizeX = null;
				SizeXWatermark = MultipleValuesPlaceholder;
			}

			if (obj?.SizeY != SizeY)
			{
				SizeY = null;
				SizeYWatermark = MultipleValuesPlaceholder;
			}

			if (obj?.Angle != Angle)
			{
				Angle = null;
				AngleWatermark = MultipleValuesPlaceholder;
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}
	}
}