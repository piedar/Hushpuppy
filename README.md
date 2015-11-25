# Hushpuppy

Hushpuppy is a set of building blocks for web-based solutions.
It includes a graphical web browser, an asynchronous HttpServer, and an HttpClient torture test.

## Hushpuppy.Browser

The Hushpuppy.Browser executable is a simple web browser using the system's native web engine.

## Hushpuppy.Server

The HttpServer class serves HTTP requests using a stateless, asynchronous model.
The Hushpuppy.Server executable hosts an HttpServer in a given directory.
New functionality may be added by implementing IHttpService.

## Hushpuppy.Torturer

The Hushpuppy.Torturer executable makes many concurrent requests to a server in order to test its responsiveness.
