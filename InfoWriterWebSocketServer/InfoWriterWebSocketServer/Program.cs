using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;
using InfoWriterWebSocketServer.Server;
using InfoWriterWebSocketServer.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace InfoWriterWebSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var ser = new ServiceCollection();
            ser.AddScoped<UserContext>();
            ser.AddScoped<TcpClient>();


            var ds = new BaseDispatcher(ser.BuildServiceProvider());
            ds.AddHandler<InfoWriterHandler>(ContextEnum.Info);
            var bs = new BaseService(ds, "127.0.0.1", 7776);
            var wssm = new WebSocketServerManager("127.0.0.1");
            wssm.AddWebSocketService(bs);
            wssm.Start();
        }
    }
}
