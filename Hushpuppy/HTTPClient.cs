using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hushpuppy
{
	public class HTTPClient
	{
		public HTTPClient()
		{
		}

		public static async Task<String> DownloadPageAsync(String uri)
		{
			using (HttpClient client = new HttpClient())
			using (HttpResponseMessage response = await client.GetAsync(uri))
			using (HttpContent content = response.Content)
			{
				String result = await content.ReadAsStringAsync();
				return result;
			}
		}

		public static async Task TortureAsync(IEnumerable<Uri> targets, Int64 requestCount)
		{
			using (HttpClient client = new HttpClient())
			{
				var pendingTasks = new List<Task>();

				foreach (Uri target in targets)
				{
					Task tortureTask = TortureAsync(client, target, requestCount);
					pendingTasks.Add(tortureTask);
				}

				await Task.WhenAll(pendingTasks);
			}
		}

		private static async Task TortureAsync(HttpClient client, Uri target, Int64 requestCount)
		{
			Int64 count = 0;
			Int64 charsRead = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();

			var pendingTasks = new List<Task<String>>();

			for (Int64 index = 0; index < requestCount; index++)
			{
				Task<String> fetchTask = client.GetStringAsync(target);

				pendingTasks.Add(fetchTask);

				/*
				using (HttpResponseMessage response = await client.GetAsync(target))
				using (HttpContent content = response.Content)
				{
					String result = await content.ReadAsStringAsync();
					lock (stopwatch)
					{
						charsRead += result.Length;
					}

					count = index + 1;
					if (count % 10 == 0)
					{
						Console.WriteLine("Read {0} results in {2} ({3:0} chars / ms)",
							count, charsRead, stopwatch.Elapsed, charsRead / stopwatch.Elapsed.TotalMilliseconds);
					}
				}
				*/
			}

			foreach (var task in pendingTasks)
			{
				String result = await task;
				charsRead += result.Length;
			}

			Console.WriteLine("Tortured {0} with {1} requests in {2} ({3:0} chars / ms)",
				target, count, stopwatch.Elapsed, charsRead / stopwatch.Elapsed.TotalMilliseconds);
		}
	}
}

