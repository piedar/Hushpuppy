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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Hushpuppy.Http.Services
{
	/// <summary>
	/// Handles a GET request by serving an index file from the requested directory.
	/// </summary>
	public class IndexFileService : IHttpService
	{
		private static readonly String[] _indexFiles =
		{
			"index.htm",
			"index.html",
			"default.htm",
			"default.html",
		};

		private readonly DirectoryInfo _root;

		public IndexFileService(DirectoryInfo root)
		{
			_root = root;
		}

		public async Task ServeAsync(HttpListenerContext context)
		{
			if (context.Request.HttpMethod != HttpMethod.GET)
			{
				throw new NotSupportedException("HttpMethod " + context.Request.HttpMethod + " not supported");
			}

			String path = context.Request.Url.ResolveLocalPath(_root);
			DirectoryInfo directory = new DirectoryInfo(path);

			List<Exception> exceptions = new List<Exception>();

			// Try to serve an index file from the directory.
			foreach (String indexFile in _indexFiles)
			{
				String indexPath = Path.Combine(directory.FullName, indexFile);
				try
				{
					FileInfo file = new FileInfo(indexPath);
					await StaticFileService.ServeFileAsync(file, context.Response).ConfigureAwait(false);
					return; // success
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			throw new AggregateException("No index files in directory " + directory.FullName, exceptions);
		}
	}
}
