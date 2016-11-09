//
//  This file is a part of Hushpuppy <https://github.com/piedar/Hushpuppy>.
//
//  Author(s):
//       Bennjamin Blast <bennjamin.blast@gmail.com>
//
//  Copyright (c) 2016 Bennjamin Blast
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
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hushpuppy.Server.Test
{
	public class TheServerHost
	{
		public TheServerHost(ITestOutputHelper output)
		{
			_output = output;
		}

		private readonly ITestOutputHelper _output;
		private readonly HttpClient _client = new HttpClient();


		internal static DirectoryInfo GetTestRootDirectory()
		{
			string[] paths = { "./www", "../www", "../../www" };
			return paths.Select(path => new DirectoryInfo(path)).First(dir => dir.Exists);
		}

		[Fact]
		public async Task Listens()
		{
			ServerSettings settings = new ServerSettings()
			{
				Protocol = "http",
				HostName = "localhost",
				RootDirectory = GetTestRootDirectory(),
			};

			using (CancellationTokenSource cancellation = new CancellationTokenSource())
			{
				ServerHost host = new ServerHost(settings);
				Task serveForever = host.ServeForeverAsync(cancellation.Token);

				Task testResponses = Task.Run(
					async () =>
					{
						var getTasks = host.Endpoints.Select(ep => _client.GetAsync(ep)).ToList();
						foreach (var task in getTasks)
						{
							using (HttpResponseMessage response = await task)
							{
								_output.WriteLine($"Response from {response.RequestMessage.RequestUri}");
								_output.WriteLine(response.ToString());

								Assert.True(response.IsSuccessStatusCode);
								Assert.Equal(HttpStatusCode.OK, response.StatusCode);
							}
						}
					});

				await Task.WhenAny(testResponses, serveForever);
			}
		}
	}
}
