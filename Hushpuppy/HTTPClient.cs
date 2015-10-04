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
	}
}

