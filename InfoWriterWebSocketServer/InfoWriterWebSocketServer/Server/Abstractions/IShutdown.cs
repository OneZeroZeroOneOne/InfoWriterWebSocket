using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.Server.Abstractions
{
    public interface IShutdown
    {
        public void OnShutdown();
    }
}
