using InfoWriterWebSocketServer.CustomHandlers;
using InfoWriterWebSocketServer.CustomModels;
using InfoWriterWebSocketServer.CustomUtilities;
using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server;
using Microsoft.Extensions.DependencyInjection;
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
            ser.AddScoped<DemoService>();
            ser.AddScoped<SessionStorage>();
            ser.AddSingleton<GlobalStorage>();


            var ds = new Dispatcher(ser.BuildServiceProvider());
            ds.AddHandler<InfoWriterHandler, InfoModel>(ContextEnum.Info);
            ds.OnShutdown<CustomOnShutdown>();
            var bs = new BaseService(ds, "127.0.0.1", 7776);
            var wssm = new WebSocketServerManager("127.0.0.1");
            wssm.AddWebSocketService(bs);
            wssm.Start();
        }
    }
}
