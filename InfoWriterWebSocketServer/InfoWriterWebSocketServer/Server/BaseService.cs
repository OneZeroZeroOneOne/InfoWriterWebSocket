using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Extentions;
using InfoWriterWebSocketServer.Models;
using InfoWriterWebSocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace InfoWriterWebSocketServer.Server
{
    public class BaseService
    {
        private string _host;
        private int _port;
        private Dispatcher _dispatcherPrototype;

        public BaseService(Dispatcher disp, string host, int port)
        {
            _host = host;
            _port = port;
            _dispatcherPrototype = disp;
        }

        
        
        public void StartListen()
        {
            Console.WriteLine("StartListen");
            IPAddress localAddr = IPAddress.Parse(_host);
            TcpListener server = new TcpListener(localAddr, _port);
            server.Start();
            Console.WriteLine($"WS Server start on {_host}:{_port}");
            Byte[] bytes = new Byte[256];
            string data = null;
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                TcpClient client = server.AcceptTcpClient();
                client.RFC6455Handshake();
                Console.WriteLine("Connected!");
                Thread myThread = new Thread(new ParameterizedThreadStart(_dispatcherPrototype.StartPolling));
                myThread.Start(client);

            }
        }
    }
}
