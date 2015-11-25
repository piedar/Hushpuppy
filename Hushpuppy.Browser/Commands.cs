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
using Eto.Forms;

namespace Hushpuppy.Browser
{
	internal sealed class NavigateCommand : Command
	{
		public NavigateCommand(BrowserWindow browser, Uri url)
		{
			MenuText = "&Navigate";
			ToolBarText = "Navigate";
			ToolTip = "Navigate to a page";
			//Icon = Icon.FromResource("name.ico");
			Shortcut = Application.Instance.CommonModifier | Keys.T;

			this.Executed +=
				(Object sender, EventArgs e) =>
				{
					BrowserTab tab = browser.CurrentTab ?? browser.AddNewTab();
					tab.Url = url;
				};
		}
	}

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
