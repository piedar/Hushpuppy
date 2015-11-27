# Hushpuppy

Hushpuppy is a set of building blocks for web-based solutions.
It includes a graphical web browser, an asynchronous HttpServer, and an HttpTorturer test.

## Hushpuppy.Browser

The Hushpuppy.Browser executable is a simple web browser using the system's native web engine.

## Hushpuppy.Http

The Hushpuppy.Http library provides an asynchronous HttpServer and its supporting IHttpService handlers.
The HttpServer class serves HTTP requests using a stateless, asynchronous model.

## Hushpuppy.Server

The Hushpuppy.Server executable hosts a Hushpuppy.Http.HttpServer in a given directory.
New functionality may be added by implementing IHttpService.

## Hushpuppy.Torturer

The Hushpuppy.Torturer executable makes many concurrent requests to a server in order to test its responsiveness.
