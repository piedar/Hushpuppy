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
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using CommandLine;
using Hushpuppy.Http;
using Hushpuppy.Http.Services;

namespace Hushpuppy.Server
{
	public class ServerSettings
	{
		public class Defaults
		{
			public const String Protocol = "http";
			public const String HostName = "localhost";
		}

		[Option(Default = Defaults.Protocol)]
		public String Protocol { get; set; } = Defaults.Protocol;

		[Option(Default = Defaults.HostName)]
		public String HostName { get; set; } = Defaults.HostName;

		[Option(HelpText = "Port for listening. If omitted, the OS assigns an unused port.")]
		public Int32? Port { get; set; }

		[Value(index: 0, HelpText = "Root directory to serve.", Required = true)]
		public DirectoryInfo RootDirectory { get; set; }
	}

	public class HostedServerSettings : ServerSettings
	{
		[Option(HelpText = "Print debug messages to the console.", Default = false)]
		public Boolean Debug { get; set; }
	}

	public class ServerHost
	{
		static Route[] GetDefaultRoutes(DirectoryInfo rootDirectory)
		{
			return new Route[]
			{
				new Route(HttpMethod.GET, "/", new StaticFileService(rootDirectory)),
				new Route(HttpMethod.GET, "/", new IndexFileService(rootDirectory)),
				new Route(HttpMethod.GET, "/", new DirectoryListingService(rootDirectory)),
			};
		}

		public ServerHost(ServerSettings settings)
			: this(settings, GetDefaultRoutes(settings.RootDirectory)) { }

		public ServerHost(ServerSettings settings, IReadOnlyCollection<Route> routes)
		{
			UriBuilder addressBuilder = new UriBuilder {
				Scheme = settings.Protocol,
				Host = settings.HostName,
				Port = settings.Port ?? HttpServer.GetUnusedPort(),
			};

			_routes.AddRange(routes);

			Uri address = addressBuilder.Uri;
			_endpoints.Add(address);
		}

		public Task ServeForeverAsync(CancellationToken token)
		{
			return HttpServer.ListenForeverAsync(Endpoints, Routes, token);
		}

		public IReadOnlyCollection<Route> Routes
		{
			get { return _routes.AsReadOnly(); }
		}
		private readonly List<Route> _routes = new List<Route>();

		public IReadOnlyCollection<Uri> Endpoints
		{
			get { return _endpoints.AsReadOnly(); }
		}
		private readonly List<Uri> _endpoints = new List<Uri>();



		static async Task MainAsync(String[] args)
		{
			CancellationTokenSource cancellation = new CancellationTokenSource();
			Console.CancelKeyPress +=
				(Object sender, ConsoleCancelEventArgs e) =>
				{
					Console.WriteLine("Handled Ctrl+C; cancelling running tasks...");
					cancellation.Cancel();
				};

			ParserResult<HostedServerSettings> result = CommandLine.Parser.Default.ParseArguments<HostedServerSettings>(args);
			HostedServerSettings settings = result.MapResult(set => set, errors => null);
			if (settings == null)
			{
				throw new ArgumentException();
			}

			if (settings.Debug)
			{
				// todo: proper logging
				Debug.Listeners.Add(new ConsoleTraceListener());
			}

			ServerHost host = new ServerHost(settings);

			Task serve = host.ServeForeverAsync(cancellation.Token);

			foreach (Uri endpoint in host.Endpoints)
			{
				Console.WriteLine(endpoint.ToString());
			}

			await serve;
		}

		static void Main(String[] args)
		{
			Task.Run(async () => await MainAsync(args)).Wait();
		}
	}
}