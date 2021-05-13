using InfoWriterWebSocketServer.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace InfoWriterWebSocketServer.Server
{
    public class UserContext
    {
        public TcpClient client;
        public Update Update { get; set; }
        public Guid Id { get; set; }
    }
}
