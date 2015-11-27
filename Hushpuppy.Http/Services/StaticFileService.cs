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
using System.Threading.Tasks;
using System.Web;

namespace Hushpuppy.Http.Services
{
	/// <summary>
	/// Handles a GET request by serving the requested file.
	/// </summary>
	public class StaticFileService : IHttpService
	{
		private readonly DirectoryInfo _root;

		public StaticFileService(DirectoryInfo root)
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
			FileInfo file = new FileInfo(path);
			return ServeFileAsync(file, context.Response);
		}

		internal static async Task ServeFileAsync(FileInfo file, HttpListenerResponse response)
		{
			using (Stream input = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				String mime = MimeMapping.GetMimeMapping(file.Name);
				response.ContentType = mime ?? "application/octet-stream";
				response.ContentLength64 = input.Length;
				response.AddHeader("Date", DateTime.Now.ToString("r"));
				response.AddHeader("Last-Modified", file.LastWriteTime.ToString("r"));

				response.StatusCode = (Int32)HttpStatusCode.OK; // Must be set before writing to OutputStream.

				await input.CopyToAsync(response.OutputStream);
			}
		}
	}
}
