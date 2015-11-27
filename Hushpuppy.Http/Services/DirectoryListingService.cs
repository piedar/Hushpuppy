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

namespace Hushpuppy.Http.Services
{
	public class DirectoryListingService : IHttpService
	{
		private readonly DirectoryInfo _root;

		public DirectoryListingService(DirectoryInfo root)
		{
			_root = root;
		}

		public Task ServeAsync(HttpListenerContext context)
		{
			if (context.Request.HttpMethod != "GET")
			{
				throw new NotSupportedException("HttpMethod " + context.Request.HttpMethod + " not supported");
			}

			String path = _root.ResolveLocalPath(context.Request.Url);
			DirectoryInfo directory = new DirectoryInfo(path);
			return ServeDirectoryListingAsync(_root, directory, context.Response);
		}

		private static async Task ServeDirectoryListingAsync(DirectoryInfo root, DirectoryInfo directory, HttpListenerResponse response)
		{
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
