using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hushpuppy
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
			Task httpd = HTTPServer.ListenAsync(root, 8080, cancellationSource.Token);
			await httpd;
		}

		static void Main(String[] args)
		{
			Task.Run(async () => await MainAsync(args)).Wait();
		}
	}
}