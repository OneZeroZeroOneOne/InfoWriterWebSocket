using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace InfoWriterWebSocketClient.Client.Abstractions
{
    public interface IMiddleware
    {
        public IHandlResult Execute();
    }
}
