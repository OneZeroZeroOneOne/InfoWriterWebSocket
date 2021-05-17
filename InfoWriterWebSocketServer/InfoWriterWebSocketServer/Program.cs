using InfoWriterWebSocketServer.CustomHandlers;
using InfoWriterWebSocketServer.CustomModels;
using InfoWriterWebSocketServer.CustomUtilities;
using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server;
using InfoWriterWebSocketServer.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace InfoWriterWebSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var ser = new ServiceCollection();
            ser.AddScoped<UserContext>();
            ser.AddScoped<TcpClient>();
            ser.AddScoped<DemoService>();
            ser.AddScoped<SessionStorage>();
            ser.AddSingleton<GlobalStorage>();
            var ds = new Dispatcher(ser.BuildServiceProvider());
            ds.AddHandler<InfoWriterHandler, InfoModel>(ContextEnum.Info);
            ds.OnShutdown<CustomOnShutdown>();
            var bs = new BaseService(ds, GetLocalIPAddress(), 7776);
            var wssm = new WebSocketServerManager();
            wssm.AddWebSocketService(bs);
            wssm.Start();
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
