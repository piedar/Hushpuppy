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
using System.Net;
using System.Threading.Tasks;

namespace Hushpuppy.Http
{
	/// <summary>
	/// Handles requests and populates responses.
	/// </summary>
	public interface IHttpService
	{
		/// <summary>
		/// Serves the given <paramref name="context"/> asynchronously.
		/// Throws an exception if the request was not handled successfully.
		/// </summary>
		/// <param name="context"></param>
		/// <exception cref="AggregateException">One or more errors occurred.</exception>
		/// <exception cref="NotSupportedException">The service does not support the given request (e.g. a GET service can't handle a POST request).</exception>
		Task ServeAsync(HttpListenerContext context);
	}
}
