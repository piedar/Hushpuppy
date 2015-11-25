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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hushpuppy
{
	public static class HttpServer
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

		public static async Task ListenAsync(IReadOnlyCollection<IHttpService> services, Int32? port = null, CancellationToken cancellation = default(CancellationToken))
		{
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
						Task serveTask = ServeAsync(context, services);
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

		private static async Task ServeAsync(HttpListenerContext context, IEnumerable<IHttpService> services)
		{
			// Services should set this when populating response.
			context.Response.StatusCode = (Int32)HttpStatusCode.InternalServerError;

			try
			{
				foreach (IHttpService service in services)
				{
					try
					{
						await service.ServeAsync(context);
						Debug.WriteLine("{0} successfully served {1}", service.ToString(), context.Request.Url);
						return; // success
					}
					catch (Exception ex)
					{
						Debug.WriteLine("{0} failed to serve {1}", service.ToString(), context.Request.Url);
						Debug.WriteLine("\t" + ex.GetType() + ": " + ex.Message);
					}
				}

				// Nobody served the request.
				context.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
			}
			finally
			{
				context.Response.Close();
			}
		}
	}
}
