using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using StudioZDR.App.Utility;

namespace StudioZDR.App.ViewModels;

public abstract class ViewModelWithDataModel<TDataModel> : ViewModelBase
{
	protected abstract TDataModel DataModel { get; }

	protected void SetDataModelValue<TValue>(Expression<Func<TDataModel, TValue>> expression, TValue value, [CallerMemberName] string? propertyName = default)
	{
		var property = ReflectionUtility.GetProperty(expression);
		var propertyValue = (TValue?) property.GetValue(DataModel);

		this.RaiseAndSetIfChanged(ref propertyValue, value, propertyName);

		property.SetValue(DataModel, propertyValue);
	}
}