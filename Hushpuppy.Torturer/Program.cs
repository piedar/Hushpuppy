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
using CommandLine;
using Hushpuppy.Http;

namespace Hushpuppy.Torturer
{
	class Options
	{
		[Value(0, HelpText="Url to torture", Required=true)]
		public Uri Target { get; set; }

		[Option(HelpText="Number of requests", Default=500)]
		public Int32 Requests { get; set; }
	}

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

			ParserResult<Options> parseResult = CommandLine.Parser.Default.ParseArguments<Options>(args);
			Options options = parseResult.MapResult(opts => opts, errors => null);
			if (options == null)
			{
				return;
			}

			var result = await HttpTorturer.TortureAsync(options.Target, options.Requests);
			Console.WriteLine(result);
		}

		static void Main(String[] args)
		{
			Task.Run(async () => await MainAsync(args)).Wait();
		}
	}
}