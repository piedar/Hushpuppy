using System;
using System.Net;
using System.Threading.Tasks;

namespace Hushpuppy
{
	interface IServiceHandler
	{
		Boolean CanServe(String path);
		Task ServeAsync(String path, HttpListenerResponse response);
	}
}

