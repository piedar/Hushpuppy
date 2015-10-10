using System;
using Eto.Forms;

namespace Hushpuppy.Browser
{
	internal sealed class NewTabCommand : Command
	{
		public NewTabCommand(BrowserWindow browser, Uri url)
		{
			MenuText = "&New Tab";
			ToolBarText = "New Tab";
			ToolTip = "Open a new tab";
			//Icon = Icon.FromResource("name.ico");
			Shortcut = Application.Instance.CommonModifier | Keys.T;

			this.Executed +=
				(Object sender, EventArgs e) =>
				{
					BrowserTab tab = browser.AddNewTab();
					tab.Url = url;
				};
		}
	}

	internal sealed class CloseTabCommand : Command
	{
		public CloseTabCommand(BrowserWindow browser)
		{
			MenuText = "&Close Tab";
			ToolBarText = "Close Tab";
			ToolTip = "Close the current tab";
			//Icon = Icon.FromResource("name.ico");
			Shortcut = Application.Instance.CommonModifier | Keys.W;

			this.Executed +=
				(Object sender, EventArgs e) =>
				{
					browser.CloseCurrentTab();
				};
		}
	}

	internal sealed class NewWindowCommand : Command
	{
		public NewWindowCommand()
		{
			MenuText = "New Window";
			ToolBarText = "New Window";
			ToolTip = "Open a new window";
			//Icon = Icon.FromResource("name.ico");
			Shortcut = Application.Instance.CommonModifier | Keys.N;

			this.Executed +=
				(Object sender, EventArgs e) =>
				{
					BrowserWindow form = new BrowserWindow();
					form.Show();
				};
		}
	}
}
