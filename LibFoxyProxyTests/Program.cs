// See https://aka.ms/new-console-template for more information

using LibFoxyProxy.Http;
using System.Net;

using var _resetEvent = new ManualResetEvent(false);

var proxy = new HttpProxy(IPAddress.Any, 1992);

proxy.Start();

_resetEvent.WaitOne();