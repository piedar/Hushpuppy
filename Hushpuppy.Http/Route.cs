//
//  This file is a part of Hushpuppy <https://github.com/piedar/Hushpuppy>.
//
//  Author(s):
//       Bennjamin Blast <bennjamin.blast@gmail.com>
//
//  Copyright (c) 2015 Bennjamin Blast
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
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace Hushpuppy.Http
{
	public class HttpMethod
	{
		private HttpMethod() { }

		public String Verb { get; private set; }

		public static implicit operator String(HttpMethod method)
		{
			return method.Verb;
		}

		public static readonly HttpMethod GET     = new HttpMethod { Verb = "GET" };
		public static readonly HttpMethod HEAD    = new HttpMethod { Verb = "HEAD" };
		public static readonly HttpMethod POST    = new HttpMethod { Verb = "POST" };
		public static readonly HttpMethod PUT     = new HttpMethod { Verb = "PUT" };
		public static readonly HttpMethod DELETE  = new HttpMethod { Verb = "DELETE" };
		public static readonly HttpMethod TRACE   = new HttpMethod { Verb = "TRACE" };
		public static readonly HttpMethod OPTIONS = new HttpMethod { Verb = "OPTIONS" };
		public static readonly HttpMethod CONNECT = new HttpMethod { Verb = "CONNECT" };
		public static readonly HttpMethod PATCH   = new HttpMethod { Verb = "PATCH" };
	}

	public class Route
	{
		public Route(HttpMethod method, String prefix, IHttpService service)
		{
			Method = method;
			Prefix = prefix;
			Service = service;
		}

		public HttpMethod Method { get; private set; }
		public String Prefix { get; private set; }
		public IHttpService Service { get; private set; }
	}
}
