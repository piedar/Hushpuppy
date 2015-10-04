using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

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

		public static IEnumerable<T> RemoveAndYieldWhere<T>(this IList<T> source, Func<T, Boolean> predicate)
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

	public static class HTTPServer
	{
		private static IDictionary<String, String> _mimeTypeMappings = new Dictionary<String, String>(StringComparer.InvariantCultureIgnoreCase)
		{
			#region extension to MIME type list
			{".asf", "video/x-ms-asf"},
			{".asx", "video/x-ms-asf"},
			{".avi", "video/x-msvideo"},
			{".bin", "application/octet-stream"},
			{".cco", "application/x-cocoa"},
			{".crt", "application/x-x509-ca-cert"},
			{".css", "text/css"},
			{".deb", "application/octet-stream"},
			{".der", "application/x-x509-ca-cert"},
			{".dll", "application/octet-stream"},
			{".dmg", "application/octet-stream"},
			{".ear", "application/java-archive"},
			{".eot", "application/octet-stream"},
			{".exe", "application/octet-stream"},
			{".flv", "video/x-flv"},
			{".gif", "image/gif"},
			{".hqx", "application/mac-binhex40"},
			{".htc", "text/x-component"},
			{".htm", "text/html"},
			{".html", "text/html"},
			{".ico", "image/x-icon"},
			{".img", "application/octet-stream"},
			{".iso", "application/octet-stream"},
			{".jar", "application/java-archive"},
			{".jardiff", "application/x-java-archive-diff"},
			{".jng", "image/x-jng"},
			{".jnlp", "application/x-java-jnlp-file"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".js", "application/x-javascript"},
			{".mml", "text/mathml"},
			{".mng", "video/x-mng"},
			{".mov", "video/quicktime"},
			{".mp3", "audio/mpeg"},
			{".mpeg", "video/mpeg"},
			{".mpg", "video/mpeg"},
			{".msi", "application/octet-stream"},
			{".msm", "application/octet-stream"},
			{".msp", "application/octet-stream"},
			{".pdb", "application/x-pilot"},
			{".pdf", "application/pdf"},
			{".pem", "application/x-x509-ca-cert"},
			{".pl", "application/x-perl"},
			{".pm", "application/x-perl"},
			{".png", "image/png"},
			{".prc", "application/x-pilot"},
			{".ra", "audio/x-realaudio"},
			{".rar", "application/x-rar-compressed"},
			{".rpm", "application/x-redhat-package-manager"},
			{".rss", "text/xml"},
			{".run", "application/x-makeself"},
			{".sea", "application/x-sea"},
			{".shtml", "text/html"},
			{".sit", "application/x-stuffit"},
			{".swf", "application/x-shockwave-flash"},
			{".tcl", "application/x-tcl"},
			{".tk", "application/x-tcl"},
			{".txt", "text/plain"},
			{".war", "application/java-archive"},
			{".wbmp", "image/vnd.wap.wbmp"},
			{".wmv", "video/x-ms-wmv"},
			{".xml", "text/xml"},
			{".xpi", "application/x-xpinstall"},
			{".zip", "application/zip"},
			#endregion
		};

		private static readonly String[] _indexFiles =
		{
			"index.html",
			"index.htm",
			"default.html",
			"default.htm"
		};

		private static Int32 ChoosePort()
		{
			// get an empty port
			TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			Int32 port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}

		public static async Task ListenAsync(DirectoryInfo root, Int32? port = null, CancellationToken? cancellation = null)
		{
			port = port ?? ChoosePort();
			cancellation = cancellation ?? new CancellationToken();

			HttpListener listener = new HttpListener();
			listener.Prefixes.Add("http://*:" + port.ToString() + "/");
			listener.Start();

			try
			{
				List<Task> pendingTasks = new List<Task>();

				while (true)
				{
					cancellation.Value.ThrowIfCancellationRequested();

					try
					{
						HttpListenerContext context = await listener.GetContextAsync();

						// Begin servicing the request.
						Task serveTask = ServeAsync(root, context).ContinueWith(task => context.Response.Close());
						pendingTasks.Add(serveTask);

						// Reap completed tasks and propogate exceptions.
						IEnumerable<Task> completedTasks = pendingTasks.RemoveAndYieldWhere(task => task.IsCompleted);
						await Task.WhenAll(completedTasks);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("HTTPServer caught exception:");
						Debug.WriteLine(ex.ToString());
					}
				}
			}
			finally
			{
				listener.Stop();
			}
		}

		private static Task ServeAsync(DirectoryInfo root, HttpListenerContext context)
		{
			String path = context.Request.Url.AbsolutePath;
			Debug.WriteLine("Requested file {0}", (Object)path);
			path = path.Substring(1);

			if (String.IsNullOrEmpty(path))
			{
				foreach (String indexFile in _indexFiles)
				{
					if (File.Exists(Path.Combine(root.FullName, indexFile)))
					{
						path = indexFile;
						break;
					}
				}
			}

			path = Path.Combine(root.FullName, path);

			if (File.Exists(path))
			{
				FileInfo file = new FileInfo(path);
				return ServeFileAsync(file, context.Response);
			}

			if (Directory.Exists(path))
			{
				DirectoryInfo directory = new DirectoryInfo(path);
				return ServeDirectoryAsync(root, directory, context.Response);
			}

			context.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
			return Task.WhenAll(); // Task.CompletedTask;
		}

		private static async Task ServeFileAsync(FileInfo file, HttpListenerResponse response)
		{
			Debug.WriteLine("Serving file {0}", (Object)file.Name);

			response.StatusCode = (Int32)HttpStatusCode.InternalServerError;

			using (Stream input = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				String mime;
				response.ContentType = _mimeTypeMappings.TryGetValue(file.Extension, out mime) ? mime : "application/octet-stream";
				response.ContentLength64 = input.Length;
				response.AddHeader("Date", DateTime.Now.ToString("r"));
				response.AddHeader("Last-Modified", file.LastWriteTime.ToString("r"));

				response.StatusCode = (Int32)HttpStatusCode.OK; // Must be set before writing to OutputStream.

				await input.CopyToAsync(response.OutputStream);
			}
		}

		private static async Task ServeDirectoryAsync(DirectoryInfo root, DirectoryInfo directory, HttpListenerResponse response)
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

			response.StatusCode = (Int32)HttpStatusCode.OK;

			using (StreamWriter writer = new StreamWriter(response.OutputStream))
			{
				await writer.WriteAsync(htmlResponse);
			}
		}
	}



	static class ConsoleProgram
	{
		static async Task MainAsync(String[] args)
		{
			DirectoryInfo root = new DirectoryInfo(@"/home/benn/httpd");
			await HTTPServer.ListenAsync(root, 8080);
		}

		static void Main(String[] args)
		{
			Task.Run(async () => await MainAsync(args)).Wait();
		}
	}
}
