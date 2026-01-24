using System.Drawing;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Features.GuiEditor.Extensions;
using StudioZDR.App.Features.GuiEditor.HelperTypes;
using StudioZDR.App.Utility;
using StudioZDR.App.ViewModels;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public partial class DisplayObjectPropertiesViewModel : ViewModelBase, IDisposable
{
	protected const string MultipleValuesPlaceholder = "<multiple>";

	private static readonly GuiTransform FullScreenTransform = GuiTransform.CreateDefault(new RectangleF(0, 0, 1, 1));

	private readonly Subject<Unit>    changes         = new();
	private readonly SerialDisposable nodesDisposable = new();

	private volatile int refreshing;

	public DisplayObjectPropertiesViewModel()
	{
		this.WhenAnyValue(m => m.Nodes)
			.Subscribe(nodes => {
				AggregateAndRefreshValues();

				this.nodesDisposable.Disposable = nodes
					?.Select(n => n.DisplayObjectChanges)
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

		this.WhenAnyValue(m => m.ScaleX)
			.Subscribe(x => SetAllValues(n => n.ScaleX, x));

		this.WhenAnyValue(m => m.ScaleY)
			.Subscribe(y => SetAllValues(n => n.ScaleY, y));

		this.WhenAnyValue(m => m.Angle)
			.Subscribe(angle => SetAllValues(n => n.Angle, angle));

		this.WhenAnyValue(m => m.Color)
			.Subscribe(SetColor);
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

	#region Scale

	[Reactive]
	public partial float? ScaleX { get; set; }

	[Reactive]
	public partial string? ScaleXWatermark { get; set; }

	[Reactive]
	public partial float? ScaleY { get; set; }

	[Reactive]
	public partial string? ScaleYWatermark { get; set; }

	#endregion

	#region Angle

	[Reactive]
	public partial float? Angle { get; set; }

	[Reactive]
	public partial string? AngleWatermark { get; set; }

	#endregion

	#region Color

	[Reactive]
	public partial Vector4? Color { get; set; }

	[Reactive]
	public partial bool ColorIsAmbiguous { get; set; }

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
		this.nodesDisposable.Dispose();
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

			var transform = GetTransform(node, out var parentTransform);
			var parentBounds = parentTransform.OrientedBoundingBox;
			var objBounds = transform.OrientedBoundingBox;

			if (anchor == Properties.HorizontalAnchor.Right)
			{
				// Take a vector from the parent's top right pointing to the object's top right, and project that
				// along the parent's "left vector", producing the object's inset in from the parent's right edge (in
				// the parent's local space).
				var parentTopRightToObjTopRight = objBounds.TopRight - parentBounds.TopRight;
				var parentLeftVector = -parentBounds.RightVector; // Need the "left vector" because positive RightX values move the object left
				var insetFromRight = Vector2.Dot(parentTopRightToObjTopRight, parentLeftVector) / parentLeftVector.Length();

				obj.RightX = insetFromRight;
				obj.X = obj.LeftX = obj.CenterX = null;
			}
			else if (anchor == Properties.HorizontalAnchor.Center)
			{
				// Take a vector from the parent's center pointing to the object's center, and project that along
				// the parent's right vector, producing the X-offset of the object from its parent's center (in the
				// parent's local space).
				var parentCenterToObjCenter = objBounds.Center - parentBounds.Center;
				var parentRightVector = parentBounds.RightVector;
				var xOffset = Vector2.Dot(parentCenterToObjCenter, parentRightVector) / parentRightVector.Length();

				obj.CenterX = xOffset;
				obj.X = obj.LeftX = obj.RightX = null;
			}
			else
			{
				// Take a vector from the parent's top left pointing to the object's top left, and project that
				// along the parent's right vector, producing the X-offset of the object from its parent's left
				// edge (in the parent's local space).
				var parentTopLeftToObjTopLeft = objBounds.TopLeft - parentBounds.TopLeft;
				var parentRightVector = parentBounds.RightVector;
				var xOffset = Vector2.Dot(parentTopLeftToObjTopLeft, parentRightVector) / parentRightVector.Length();

				obj.X = xOffset;
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

			var transform = GetTransform(node, out var parentTransform);
			var parentBounds = parentTransform.OrientedBoundingBox;
			var objBounds = transform.OrientedBoundingBox;

			if (anchor == Properties.VerticalAnchor.Bottom)
			{
				// Take a vector from the parent's bottom left pointing to the object's bottom left, and project that
				// along the parent's "up vector", producing the object's inset in from the parent's bottom edge (in
				// the parent's local space).
				var parentBottomLeftToObjBottomLeft = objBounds.BottomLeft - parentBounds.BottomLeft;
				var parentUpVector = -parentBounds.DownVector; // Need the "up vector" because positive BottomY values move the object up
				var insetFromBottom = Vector2.Dot(parentBottomLeftToObjBottomLeft, parentUpVector) / parentUpVector.Length();

				obj.BottomY = insetFromBottom;
				obj.Y = obj.TopY = obj.CenterY = null;
			}
			else if (anchor == Properties.VerticalAnchor.Center)
			{
				// Take a vector from the parent's center pointing to the object's center, and project that along
				// the parent's down vector, producing the Y-offset of the object from its parent's center (in the
				// parent's local space).
				var parentCenterToObjCenter = objBounds.Center - parentBounds.Center;
				var parentDownVector = parentBounds.DownVector;
				var yOffset = Vector2.Dot(parentCenterToObjCenter, parentDownVector) / parentDownVector.Length();

				obj.CenterY = yOffset;
				obj.Y = obj.TopY = obj.BottomY = null;
			}
			else
			{
				// Take a vector from the parent's top left pointing to the object's top left, and project that
				// along the parent's down vector, producing the Y-offset of the object from its parent's top
				// edge (in the parent's local space).
				var parentTopLeftToObjTopLeft = objBounds.TopLeft - parentBounds.TopLeft;
				var parentDownVector = parentBounds.DownVector;
				var yOffset = Vector2.Dot(parentTopLeftToObjTopLeft, parentDownVector) / parentDownVector.Length();

				obj.Y = yOffset;
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

			var horizontalAnchor = obj.GetHorizontalAnchor();
			var verticalAnchor = obj.GetVerticalAnchor();

			node.DelayChangeNotifications().DisposeWith(delayedNotifications);

			obj.X = obj.LeftX = obj.CenterX = obj.RightX = null;
			obj.Y = obj.TopY = obj.CenterY = obj.BottomY = null;

			Expression<Func<GUI__CDisplayObject, float?>> xPropertyExpression = horizontalAnchor switch {
				Properties.HorizontalAnchor.Right  => o => o.RightX,
				Properties.HorizontalAnchor.Center => o => o.CenterX,
				_                                  => o => o.X,
			};
			Expression<Func<GUI__CDisplayObject, float?>> yPropertyExpression = verticalAnchor switch {
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

	private void SetColor(Vector4? color)
	{
		if (IsRefreshing) // Do NOT set properties on models while we're freshing FROM the models!
			return;
		if (Nodes is null or [])
			return;

		var didChange = false;
		using var delayedNotifications = new CompositeDisposable();

		foreach (var node in Nodes)
		{
			if (node.DisplayObject is not { } obj)
				continue;

			node.DelayChangeNotifications().DisposeWith(delayedNotifications);

			var prevColor = obj.GetColor();

			if (color == prevColor)
				continue;

			obj.SetColor(color);
			node.NotifyOfDisplayObjectChange();
			didChange = true;
		}

		if (didChange)
			this.changes.OnNext(Unit.Default);
	}

	private static GuiTransform GetTransform(GuiCompositionNodeViewModel? node, out GuiTransform parentTransform)
	{
		if (node?.DisplayObject is not { } obj)
		{
			parentTransform = FullScreenTransform;
			return FullScreenTransform;
		}

		parentTransform = GetTransform(node.Parent, out _);

		return obj.GetTransform(parentTransform);
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

				RefreshValuesFromObject(obj, firstObject: i == 0);
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
		ScaleX = null;
		ScaleXWatermark = null;
		ScaleY = null;
		ScaleYWatermark = null;
		Angle = null;
		AngleWatermark = null;
		Color = null;
		ColorIsAmbiguous = false;
	}

	protected virtual void RefreshValuesFromObject(GUI__CDisplayObject? obj, bool firstObject)
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

		if (firstObject)
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
			ScaleX = obj?.ScaleX;
			ScaleY = obj?.ScaleY;
			Angle = obj?.Angle;
			Color = obj?.GetColor();
			ColorIsAmbiguous = false;
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

			if (obj?.ScaleX != ScaleX)
			{
				ScaleX = null;
				ScaleXWatermark = MultipleValuesPlaceholder;
			}

			if (obj?.ScaleY != ScaleY)
			{
				ScaleY = null;
				ScaleYWatermark = MultipleValuesPlaceholder;
			}

			if (obj?.Angle != Angle)
			{
				Angle = null;
				AngleWatermark = MultipleValuesPlaceholder;
			}

			if (obj?.GetColor() != Color)
			{
				Color = null;
				ColorIsAmbiguous = true;
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}
	}
}