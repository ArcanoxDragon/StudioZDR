using System.Numerics;

namespace StudioZDR.App.Utility;

public static class MathHelper
{
	public static T MinAll<T>(params IEnumerable<T> values)
	where T : struct, INumber<T>
	{
		var minimum = default(T);
		var first = true;

		foreach (var value in values)
		{
			if (first || value < minimum)
				minimum = value;

			first = false;
		}

		return minimum;
	}

	public static T MaxAll<T>(params IEnumerable<T> values)
	where T : struct, INumber<T>
	{
		var maximum = default(T);
		var first = true;

		foreach (var value in values)
		{
			if (first || value > maximum)
				maximum = value;

			first = false;
		}

		return maximum;
	}
}