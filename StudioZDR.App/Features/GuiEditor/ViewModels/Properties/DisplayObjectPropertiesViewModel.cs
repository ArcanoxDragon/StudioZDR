using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using DynamicData.Binding;
using MercuryEngine.Data.Types.DreadTypes;
using ReactiveUI.SourceGenerators;
using StudioZDR.App.Utility;
using StudioZDR.App.ViewModels;

namespace StudioZDR.App.Features.GuiEditor.ViewModels.Properties;

public sealed partial class DisplayObjectPropertiesViewModel : ViewModelBase, IDisposable
{
	private const string MultipleValuesPlaceholder = "<multiple>";

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
	}

	public IObservable<Unit> Changes => this.changes;

	[Reactive]
	public partial IList<GuiCompositionNodeViewModel>? Nodes { get; set; }

	[Reactive]
	public partial bool CanEdit { get; set; }

	[Reactive]
	public partial string? Id { get; set; }

	[Reactive]
	public partial string? IdWatermark { get; set; }

	[Reactive]
	public partial bool? IsVisible { get; set; }

	public void Dispose()
	{
		this.changes.Dispose();
		this.collectionDisposable.Dispose();
	}

	private void SetAllValues<T>(Expression<Func<GUI__CDisplayObject, T>> propertyExpression, T value)
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

			if (!Equals(currentValue, value))
			{
				property.SetValue(obj, value);
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

			if (Nodes != null)
			{
				for (var i = 0; i < Nodes.Count; i++)
				{
					var obj = Nodes[i].DisplayObject;

					if (i == 0)
					{
						Id = obj?.ID;
						IsVisible = obj?.Visible;
					}
					else
					{
						if (obj?.ID != Id)
						{
							Id = null;
							IdWatermark = MultipleValuesPlaceholder;
						}

						if (obj?.Visible != IsVisible)
							IsVisible = null;
					}
				}
			}
		}
		finally
		{
			Interlocked.Decrement(ref this.refreshing);
		}
	}
}