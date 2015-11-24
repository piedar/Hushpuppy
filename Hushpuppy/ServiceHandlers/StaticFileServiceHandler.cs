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
