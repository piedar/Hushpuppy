using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Hushpuppy;

namespace Hushpuppy.Torturer
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

			var targets = new []
			{
				new Uri("http://localhost:8080/storage/www/TortureTargets/dlanham-FireyFox.jpg"),
				//new Uri("http://localhost:8080/storage/www/TortureTargets/The%20Character%20of%20Physical%20Law%201%20-%20The%20Law%20of%20Gravitation.mp4"),
				//new Uri("http://localhost:8080/storage/images/wallpaper/dlanham-FireyFox.jpg"),
			};

			Task httpdTorturer = HTTPClient.TortureAsync(targets, 500);
			await httpdTorturer;
		}

		static void Main(String[] args)
		{
			Task.Run(async () => await MainAsync(args)).Wait();
		}
	}
}