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
			if (context.Request.HttpMethod != HttpMethod.GET)
			{
				throw new NotSupportedException("HttpMethod " + context.Request.HttpMethod + " not supported");
			}

			String path = context.Request.Url.ResolveLocalPath(_root);
			FileInfo file = new FileInfo(path);
			return ServeFileAsync(file, context.Request, context.Response);
		}

		internal static async Task ServeFileAsync(FileInfo file, HttpListenerRequest request, HttpListenerResponse response)
		{
			using (Stream input = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				String mime = file.GetMimeType();
				response.ContentType = mime ?? "application/octet-stream";
				response.ContentLength64 = input.Length;
				response.Headers["Date"] = DateTime.Now.ToString("r");
				response.Headers["Last-Modified"] = file.LastWriteTime.ToString("r");
				response.Headers["Accept-Ranges"] = "bytes";

				response.StatusCode = (Int32)HttpStatusCode.OK; // Must be set before writing to OutputStream.

				long length = file.Length;

				long start;
				long? end;
				if (!TryGetBytesRange(request, out start, out end))
				{
					start = 0;
					end = null;
				}

				end = end ?? (input.Length - 1);
				length = end.Value - start + 1;

				if (length < 0)
				{
					throw new InvalidOperationException("Unexpected length: " + length);
				}

				response.StatusCode = (Int32)HttpStatusCode.PartialContent;
				response.Headers["Content-Length"] = length.ToString();
				response.Headers["Content-Range"] = $"bytes {start}-{end}/{file.Length}";

				input.Position = start;

				// todo: extension method
				int bytesRead = 0;
				long bytesRemaining = length;
				Byte[] buffer = new Byte[8192];
				while (bytesRemaining > 0 &&
				       (bytesRead = await input.ReadAsync(buffer, 0, Convert.ToInt32(Math.Min(buffer.Length, bytesRemaining))).ConfigureAwait(false)) > 0)
				{
					await response.OutputStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
					bytesRemaining -= bytesRead;
				}
			}
		}

		private static bool TryGetBytesRange(HttpListenerRequest request, out long start, out long? end)
		{
			start = 0;
			end = null;

			String range = request.Headers.Get("Range");
			if (!String.IsNullOrWhiteSpace(range))
			{
				String[] rangeParts = range.Split('=');
				if (rangeParts.Length >= 2)
				{
					if (rangeParts[0].Equals("bytes", StringComparison.InvariantCultureIgnoreCase))
					{
						String[] bytesRangeParts = rangeParts[1].Split('-');
						if (bytesRangeParts.Length >= 1)
						{
							Int64.TryParse(bytesRangeParts[0], out start);

							if (bytesRangeParts.Length >= 2)
							{
								long realEnd;
								if (Int64.TryParse(bytesRangeParts[1], out realEnd))
								{
									end = realEnd;
								}
							}

							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
