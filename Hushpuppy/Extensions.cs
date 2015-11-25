//
//  This file is a part of Hushpuppy <https://github.com/piedar/Hushpuppy>.
//
//  Author(s):
//       Bennjamin Blast
//
//  Copyright (c) 2015 Bennjamin Blast <bennjamin.blast@gmail.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
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

		public static String ResolveLocalPath(this DirectoryInfo root, Uri url)
		{
			String relativePath = Uri.UnescapeDataString(url.AbsolutePath).TrimStart('/');
			String fullPath = Path.GetFullPath(Path.Combine(root.FullName, relativePath));
			if (!fullPath.StartsWith(root.FullName))
			{
				return String.Empty; // Don't allow access to path outside of root directory.
			}
			return fullPath;
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