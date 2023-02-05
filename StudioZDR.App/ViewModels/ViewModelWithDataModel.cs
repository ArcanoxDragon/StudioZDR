using MercuryEngine.Data.Core.Utility;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace StudioZDR.App.ViewModels;

public abstract class ViewModelWithDataModel<TDataModel> : ViewModelBase
{
	protected abstract TDataModel DataModel { get; }

	protected void SetDataModelValue<TValue>(Expression<Func<TDataModel, TValue>> expression, TValue value, [CallerMemberName] string? propertyName = default)
	{
		var property = ExpressionUtility.GetProperty(expression);
		var propertyValue = (TValue?) property.GetValue(DataModel);

		this.RaiseAndSetIfChanged(ref propertyValue, value, propertyName);

		property.SetValue(DataModel, propertyValue);
	}
}