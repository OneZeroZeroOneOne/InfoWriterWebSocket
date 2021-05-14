using InfoWriterWebSocketClient.Client;
using InfoWriterWebSocketClient.Client.Enums;
using InfoWriterWebSocketClient.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfoWriterWebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var listServices = new List<BaseClient>();
            var ser = new ServiceCollection();
            ser.AddScoped<InfoStorage>();
            var serviceProvider = ser.BuildServiceProvider();
            var scope1 = serviceProvider.CreateScope();
            var bc1 = new BaseClient("127.0.0.1", 7776, scope1.ServiceProvider);
            bc1.RegisterMiddleware<InfoWriterMiddleware>();
            bc1.RegisterHandler<InfoWriterHandler>(ContextEnum.InfoStatusResponce);
            bc1.Connect();
            bc1.StartPoling();
            /*var scope2 = serviceProvider.CreateScope();
            var bc2 = new BaseClient(new Uri("ws://127.0.0.1:7775"), scope2.ServiceProvider);
            bc2.RegisterMiddleware<InfoWriterMiddleware>();
            bc2.RegisterHandler<InfoWriterHandler>(ContextEnum.InfoStatusResponce);
            listServices.Add(bc1);
            listServices.Add(bc2);
            /*foreach (var service in listServices)
            {
                await service.Connect();
                await service.StartPoling();
                
            }*/
        }
    }
}
