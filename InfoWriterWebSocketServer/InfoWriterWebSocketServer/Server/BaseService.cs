using System;
using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using InfoWriterWebSocketServer.Extentions;
using InfoWriterWebSocketServer.Utils;
using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;

namespace InfoWriterWebSocketServer.Server
{
    public class BaseService
    {
        private string _host;
        private int _port;
        private float _timeoutSec = 5;

        public void SetHostPort(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void SetTimeoutSec(float timeInSecFloat)
        {
            _timeoutSec = timeInSecFloat;
        }

        public virtual void OnMessage(Update u)
        {
            Console.WriteLine(u.Payload);
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
                Console.WriteLine("Connected!");
                Thread myThread = new Thread(new ParameterizedThreadStart(StartPolling));
                myThread.Start(client);

            }

        }

        public void StartPolling(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            client.RFC6455Handshake();
            var updateParser = new UpdateParser();
            var lastMessageTimeSec = DateTimeOffset.Now.ToUnixTimeSeconds();
            while (true)
            {
                if(DateTimeOffset.Now.ToUnixTimeSeconds() - lastMessageTimeSec > _timeoutSec)
                {
                    Console.WriteLine("the client is not responding. detachment");
                    stream.Close();
                    client.Close();
                    break;
                }
                if (client.Available > 0)
                {

                    byte[] bytes = new byte[client.Available];
                    stream.Read(bytes, 0, client.Available);
                    updateParser.SetBytes(bytes);
                    var update = updateParser.Parse();
                    Console.WriteLine($"update frame - {update.Frame}");
                    if(update.Frame == FrameMessageEnum.Ping)
                    {
                        Ping(client);
                    }
                    if(update.Frame == FrameMessageEnum.Text)
                    {
                        OnMessage(update);
                    }
                    lastMessageTimeSec = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
            }
            
        }

        public void Ping(TcpClient client)
        {
            var stream = client.GetStream();
            stream.Write(ResponseFactory.Ping());
        }

    }
}
