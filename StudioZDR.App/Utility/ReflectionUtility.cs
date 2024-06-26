using System.Linq.Expressions;
using System.Reflection;

namespace StudioZDR.App.Utility;

internal static class ReflectionUtility
{
	public static PropertyInfo GetProperty(LambdaExpression expression)
	{
		if (expression is not { Body: MemberExpression { Member: PropertyInfo propertyInfo } })
			throw new ArgumentException("Expression must be a simple property access expression", nameof(expression));

		return propertyInfo;
	}
}