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
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace Hushpuppy.Browser
{
	class BrowserToolbar : Panel
	{
		private readonly TextBox _urlBox = new TextBox();

		public BrowserToolbar()
		{
			_urlBox.KeyDown +=
				(Object sender, KeyEventArgs e) =>
				{
					if (e.Key == Keys.Enter)
					{
						TextBox urlBox = (TextBox)sender;
						BrowserWindow browser = urlBox.FindParent<BrowserWindow>();

						UriBuilder builder = new UriBuilder(urlBox.Text);
						new NavigateCommand(browser, builder.Uri).Execute();
					}
				};

			DynamicLayout layout = new DynamicLayout();
			layout.AddColumn(_urlBox);

			this.Content = layout;
		}

		public Uri Url
		{
			set { _urlBox.Text = value.ToString(); }
		}
	}

	internal sealed class BrowserWindow : Form
	{
		private readonly TabControl _tabControl = new TabControl();
		private readonly Uri _homePage = new Uri("https://google.com/");

		public BrowserWindow()
		{
			this.Title = "Hushpuppy";
			this.Size = new Size(1280, 800);

			this.Menu = new MenuBar();
			ButtonMenuItem fileMenu = Menu.Items.GetSubmenu("&File");
			fileMenu.Items.AddRange(new Command[] { new NewTabCommand(this, _homePage), new CloseTabCommand(this), new NewWindowCommand(), });

			BrowserToolbar = new BrowserToolbar();

			DynamicLayout layout = new DynamicLayout();
			layout.AddColumn(BrowserToolbar, _tabControl);

			this.Content = layout;

			new NavigateCommand(this, _homePage).Execute();
		}

		internal BrowserToolbar BrowserToolbar
		{
			get; private set;
		}

		public BrowserTab CurrentTab
		{
			get { return _tabControl.SelectedPage as BrowserTab; }
		}

		public BrowserTab AddNewTab()
		{
			BrowserTab tab = new BrowserTab();
			_tabControl.Pages.Add(tab);
			_tabControl.SelectedPage = tab;
			return tab;
		}

		public Boolean CloseTab(BrowserTab tab)
		{
			if (tab == null)
			{
				throw new ArgumentNullException("tab");
			}

			tab.StopLoading();

			Int32 index = _tabControl.Pages.IndexOf(tab);
			Boolean removed = _tabControl.Pages.Remove(tab);

			if (_tabControl.SelectedPage == tab)
			{
				Int32 nextIndex = Math.Max(0, index - 1);
				_tabControl.SelectedPage = _tabControl.Pages.ElementAtOrDefault(nextIndex);
			}

			return removed;
		}

		public Boolean CloseCurrentTab()
		{
			BrowserTab currentTab = CurrentTab;
			if (currentTab == null)
			{
				return false;
			}
			return CloseTab(currentTab);
		}
	}

	static class Program
	{
		[STAThread]
		static void Main()
		{
			using (Application application = new Application())
			{
				BrowserWindow form = new BrowserWindow();
				application.Run(form);
			}
		}
	}
}
