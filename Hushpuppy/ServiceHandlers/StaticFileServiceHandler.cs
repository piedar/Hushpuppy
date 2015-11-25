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

namespace Hushpuppy
{
	class StaticFileServiceHandler : IServiceHandler
	{
		public StaticFileServiceHandler() { }

		public Boolean CanServe(String path)
		{
			return File.Exists(path);
		}

		public Task ServeAsync(String path, HttpListenerResponse response)
		{
			FileInfo file = new FileInfo(path);
			return ServeFileAsync(file, response);
		}

		private static async Task ServeFileAsync(FileInfo file, HttpListenerResponse response)
		{
			Debug.WriteLine("Serving file {0}", (Object)file.Name);

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
