using System;
using System.Collections.Generic;
using System.IO;

namespace Hushpuppy
{
	static class Extensions
	{
		public static Boolean IsSameDirectory(this DirectoryInfo directory1, DirectoryInfo directory2)
		{
			return (
				0 == String.Compare(
					Path.GetFullPath(directory1.FullName).TrimEnd('\\'),
					Path.GetFullPath(directory2.FullName).TrimEnd('\\'),
					StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Removes and enumerates items from <paramref name="source"/> where <paramref name="predicate"/> returns true or is null.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		public static IEnumerable<T> ConsumeWhere<T>(this IList<T> source, Func<T, Boolean> predicate)
		{
			for (Int32 i = 0; i < source.Count; i++)
			{
				T item = source[i];
				if (predicate == null || predicate(item))
				{
					source.RemoveAt(i);
					i--;
					yield return item;
				}
			}
		}
	}
}