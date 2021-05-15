using InfoWriterWebSocketClient.Client.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace InfoWriterWebSocketClient.Client.Models
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
