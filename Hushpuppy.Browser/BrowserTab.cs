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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace Hushpuppy.Browser
{
	internal sealed class BrowserTab : TabPage
	{
		private readonly WebView _webView = new WebView();

		public BrowserTab()
		{
			//_webView.BrowserContextMenuEnabled = true;

			_webView.DocumentTitleChanged +=
				(Object sender, WebViewTitleEventArgs e) =>
				{
					this.Text = e.Title;
				};

			_webView.Navigated +=
				async (Object sender, WebViewLoadedEventArgs e) =>
				{
					//BrowserWindow browser = this.FindParent<BrowserWindow>();
					//browser.BrowserToolbar.Url = e.Uri;

					this.Image = await FetchFaviconAsync(e.Uri);
				};

			this.Content = _webView;
		}

		public Uri Url
		{
			get { return _webView.Url; }
			set { _webView.Url = value; }
		}

		public void StopLoading()
		{
			_webView.Stop();
		}


		private static async Task<Image> FetchFaviconAsync(Uri url)
		{
			try
			{
				try
				{
					Uri iconUrl = new Uri(new Uri(url.GetLeftPart(UriPartial.Authority)), "favicon.ico");
					Debug.WriteLine("Fetching favicon directly from {0}", iconUrl);
					return await FetchImageAsync(iconUrl);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Failed fetching favicon directly because {0}", ex);
					Debug.WriteLine("Trying again with Google s2 service");
					String iconUrl = String.Format("https://www.google.com/s2/favicons?domain_url={0}", url.Host);
					return await FetchImageAsync(new Uri(iconUrl));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed fetching favicon because {0}", ex);
				return null;
			}
		}

		private static async Task<Image> FetchImageAsync(Uri url)
		{
			using (HttpClient client = new HttpClient())
			using (HttpResponseMessage response = await client.GetAsync(url))
			using (HttpContent content = response.Content)
			{
				using (Stream stream = await content.ReadAsStreamAsync())
				{
					return new Bitmap(stream);
				}
			}
		}
	}
}
