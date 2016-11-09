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
using System.Text;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Hushpuppy.Http
{
	public static class HttpTorturer
	{
		internal static async Task<String> DownloadPageAsync(String uri)
		{
			using (HttpClient client = new HttpClient())
			using (HttpResponseMessage response = await client.GetAsync(uri))
			using (HttpContent content = response.Content)
			{
				String result = await content.ReadAsStringAsync();
				return result;
			}
		}

		public class TortureResult
		{
			public Uri Target { get; internal set; }

			public Int64 RequestCount { get; internal set; }

			public Int64 BytesRead { get; internal set; }

			public TimeSpan TimeElapsed { get; internal set; }

			public TimeSpan AverageTimePerRequest
			{
				get { return TimeSpan.FromMilliseconds(TimeElapsed.TotalMilliseconds / RequestCount); }
			}

			public override string ToString()
			{
				return string.Format("[TortureResult: Target={0}, RequestCount={1}, BytesRead={2}, TimeElapsed={3}, AverageTimePerRequest={4}]", Target, RequestCount, BytesRead, TimeElapsed, AverageTimePerRequest);
			}
		}

		/// <summary>
		/// Concurrently requests the given <paramref name="target"/> <paramref name="requestCount"/> times.
		/// </summary>
		/// <param name="target">Uri to torture.</param>
		/// <param name="requestCount">Number of requests.</param>
		/// <param name="concurrentRequestLimit">Maximum number of concurrent requests.</param>
		public static async Task<TortureResult> TortureAsync(Uri target, Int64 requestCount, Int64 concurrentRequestLimit = 22)
		{
			Int64 count = 0;
			Int64 bytesRead = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();

			var pendingTasks = new List<Task<Byte[]>>();

			using (HttpClient client = new HttpClient())
			{
				for (Int64 index = 0; index < requestCount; index++)
				{
					count = index + 1;

					Task<Byte[]> fetchTask = client.GetByteArrayAsync(target);
					pendingTasks.Add(fetchTask);

					if (pendingTasks.Count > concurrentRequestLimit)
					{
						foreach (var task in pendingTasks.ConsumeWhere(null))
						{
							Byte[] result = await task;
							bytesRead += result.LongLength;
						}
					}
				}

				foreach (var task in pendingTasks.ConsumeWhere(null))
				{
					Byte[] result = await task;
					bytesRead += result.LongLength;
				}
			}

			return new TortureResult { Target = target, RequestCount = requestCount, BytesRead = bytesRead, TimeElapsed = stopwatch.Elapsed };
		}
	}
}

