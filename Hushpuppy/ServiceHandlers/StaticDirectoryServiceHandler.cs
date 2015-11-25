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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Hushpuppy
{
	class StaticDirectoryServiceHandler : IServiceHandler
	{
		private static readonly String[] _indexFiles =
		{
			"index.htm",
			"index.html",
			"default.htm",
			"default.html",
		};

		private readonly DirectoryInfo _root;

		public StaticDirectoryServiceHandler(DirectoryInfo root)
		{
			_root = root;
		}

		public Boolean CanServe(String path)
		{
			return Directory.Exists(path);
		}

		public Task ServeAsync(String path, HttpListenerResponse response)
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			return ServeDirectoryAsync(_root, directory, response);
		}

		private static async Task ServeDirectoryAsync(DirectoryInfo root, DirectoryInfo directory, HttpListenerResponse response)
		{
			// Try to serve an index file from the directory.
			StaticFileServiceHandler fileHandler = new StaticFileServiceHandler();
			foreach (String indexFile in _indexFiles)
			{
				String path = Path.Combine(directory.FullName, indexFile);
				if (fileHandler.CanServe(path))
				{
					await fileHandler.ServeAsync(path, response);
					return;
				}
			}

			// Serve a directory listing.
			Debug.WriteLine("Serving directory {0}", (Object)directory.FullName);

			StringBuilder listBuilder = new StringBuilder();

			foreach (FileInfo file in directory.EnumerateFiles())
			{
				String target = directory.IsSameDirectory(root) ? file.Name
				                                                : Path.Combine(directory.Name, file.Name);
				listBuilder.AppendFormat("<li><a href=\"{0}\">{1}</a></li>", target, file.Name);
			}

			foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
			{
				String target = directory.IsSameDirectory(root) ? subDirectory.Name
				                                                : Path.Combine(directory.Name, subDirectory.Name);
				listBuilder.AppendFormat("<li><a href=\"{0}\">{1}</a></li>", target, subDirectory.Name);
			}

			String htmlResponse = String.Format("<ul>{0}</ul>", listBuilder.ToString());

			response.ContentType = "text/html";
			response.ContentLength64 = htmlResponse.Length;
			response.AddHeader("Date", DateTime.Now.ToString("r"));

			response.StatusCode = (Int32)HttpStatusCode.OK; // Must be set before writing to OutputStream.

			using (StreamWriter writer = new StreamWriter(response.OutputStream))
			{
				await writer.WriteAsync(htmlResponse);
			}
		}
	}
}
