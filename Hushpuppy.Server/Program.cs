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
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Hushpuppy;
using Hushpuppy.Services;

namespace Hushpuppy.Server
{
	static class ConsoleProgram
	{
		static async Task MainAsync(String[] args)
		{
			CancellationTokenSource cancellationSource = new CancellationTokenSource();
			Console.CancelKeyPress +=
				(Object sender, ConsoleCancelEventArgs e) =>
				{
					Console.WriteLine("Handled Ctrl+C; cancelling running tasks...");
					cancellationSource.Cancel();
				};

			DirectoryInfo root = new DirectoryInfo(@"/home/benn/httpd");
			var services = new IHttpService[]
			{
				new StaticFileService(root),
				new IndexFileService(root),
				new DirectoryListingService(root),
			};

			Task httpd = HttpServer.ListenAsync(services, 8080, cancellationSource.Token);
			await httpd;
		}

		static void Main(String[] args)
		{
			Task.Run(async () => await MainAsync(args)).Wait();
		}
	}
}