﻿namespace StudioZDR.App.Extensions;

public static class LinqExtensions
{
	/// <summary>
	/// Returns a sequence of tuples for each item in <paramref name="source"/>, with each tuple
	/// containing the index of that item as the first element and the item itself as the second.
	/// </summary>
	public static IEnumerable<(int index, T item)> Pairs<T>(this IEnumerable<T> source)
	{
		var index = 0;

		foreach (var item in source)
			yield return ( index++, item );
	}

	/// <summary>
	/// Returns a sequence of all items from <paramref name="source"/> that are not null.
	/// </summary>
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
	where T : class
		=> source.Where(item => item is not null)!;
}