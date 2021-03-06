using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace InfoWriterWebSocketServer.Server.Models
{
    public class UserContext
    {
        public ContextEnum ContextEnum;
        public Update Update { get; set; }

        public ConcurrentQueue<byte[]> QueueMessage = new ConcurrentQueue<byte[]>();

        public TcpClient Client;

        public CancellationToken CancellationThreadToken;


    }
}
