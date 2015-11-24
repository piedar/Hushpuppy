using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hushpuppy
{
	public static class HTTPServer
	{
		private static Int32 ChoosePort()
		{
			// get an empty port
			TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			Int32 port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}

		public static async Task ListenAsync(DirectoryInfo root, Int32? port = null, CancellationToken cancellation = default(CancellationToken))
		{
			var serviceHandlers = new IServiceHandler[]
			{
				new StaticFileServiceHandler(),
				new StaticDirectoryServiceHandler(root),
			};

			port = port ?? ChoosePort();

			HttpListener listener = new HttpListener();
			listener.Prefixes.Add("http://*:" + port.ToString() + "/");
			listener.Start();

			List<Task> pendingTasks = new List<Task>();

			try
			{
				while (true)
				{
					cancellation.ThrowIfCancellationRequested();

					try
					{
						HttpListenerContext context = await listener.GetContextAsync();

						// Begin serving the request.
						Task serveTask = ServeAsync(root, context, serviceHandlers).ContinueWith((task) => context.Response.Close());
						pendingTasks.Add(serveTask);

						// Reap completed tasks and propagate exceptions.
						IEnumerable<Task> completedTasks = pendingTasks.ConsumeWhere(task => task.IsCompleted);
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
				await Task.WhenAll(pendingTasks);
			}
		}

		private static async Task ServeAsync(DirectoryInfo root, HttpListenerContext context, IEnumerable<IServiceHandler> serviceHandlers)
		{
			String path = Uri.UnescapeDataString(context.Request.Url.AbsolutePath);
			Debug.WriteLine("Requested path {0}", (Object)path);
			path = path.TrimStart('/');

			path = ResolveLocalPath(root, path);

			// Handlers should set this when populating response.
			context.Response.StatusCode = (Int32)HttpStatusCode.InternalServerError;

			foreach (IServiceHandler handler in serviceHandlers)
			{
				try
				{
					if (handler.CanServe(path))
					{
						await handler.ServeAsync(path, context.Response);
						return; // success
					}
				}
				catch (Exception ex) when (ShouldIgnoreServiceException(ex)) { }
				catch (Exception ex)
				{
					Debug.WriteLine("Service handler {0} failed to serve {1} because {2}", handler.GetType().Name, path, ex.Message);
				}
			}

			// None of the handlers served the request.
			context.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
		}

		private static String ResolveLocalPath(DirectoryInfo root, String relativePath)
		{
			String fullPath = Path.GetFullPath(Path.Combine(root.FullName, relativePath));
			if (!fullPath.StartsWith(root.FullName))
			{
				return String.Empty; // Don't allow access to path outside of root directory.
			}
			return fullPath;
		}

		private static Boolean ShouldIgnoreServiceException(Exception ex)
		{
			SocketException socketException = (ex as SocketException) ?? (ex.InnerException as SocketException);
			if (socketException != null)
			{
				return (socketException.SocketErrorCode == SocketError.ConnectionReset);
			}
			return false;
		}
	}
}
