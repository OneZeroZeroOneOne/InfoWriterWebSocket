using InfoWriterWebSocketServer.Server;
using System;

namespace InfoWriterWebSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var wssm = new WebSocketServerManager("127.0.0.1");
            wssm.AddWebSocketService<BaseService>(7776);
            wssm.Start();
            Console.WriteLine("Hello World!");
        }
    }
}
