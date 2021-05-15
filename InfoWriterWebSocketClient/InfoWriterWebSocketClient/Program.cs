using InfoWriterWebSocketClient.Client;
using InfoWriterWebSocketClient.Client.Enums;
using InfoWriterWebSocketClient.Client.Models;
using InfoWriterWebSocketClient.Handlers;
using InfoWriterWebSocketClient.ParallelChecks;
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
            var ser = new ServiceCollection();
            ser.AddScoped<UserContext>();



            var listServices = new List<BaseClient>();
            ser.AddScoped<InfoStorage>();
            var serviceProvider = ser.BuildServiceProvider();
            var scope1 = serviceProvider.CreateScope();
            var bc1 = new BaseClient("127.0.0.1", 7776, scope1.ServiceProvider);
            bc1.RegisterParallelCheck<CustomParallelCheck>();
            bc1.RegisterHandler<InfoWriterHandler>(ContextEnum.InfoStatusResponce);
            bc1.Connect();
            bc1.StartPoling();
        }
    }
}
