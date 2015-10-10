using System;
using System.Diagnostics;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace Hushpuppy.Browser
{
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

			new NewTabCommand(this, _homePage).Execute();

			TextBox urlBox = new TextBox();
			urlBox.KeyDown +=
				(Object sender, KeyEventArgs e) =>
				{
					if (e.Key == Keys.Enter)
					{
						BrowserTab currentTab = this.CurrentTab ?? this.AddNewTab();
						UriBuilder builder = new UriBuilder(urlBox.Text);
						currentTab.Url = builder.Uri;
					}
				};

			DynamicLayout layout = new DynamicLayout();
			layout.AddColumn(urlBox, _tabControl);

			this.Content = layout;
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

			tab.Stop();

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
