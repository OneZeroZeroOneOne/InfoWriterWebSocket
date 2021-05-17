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
            Console.WriteLine("please type server url");
            var url = Console.ReadLine();
            var listServices = new List<BaseClient>();
            ser.AddScoped<InfoStorage>();
            var serviceProvider = ser.BuildServiceProvider();
            var bc = new BaseClient(url, 7776, serviceProvider);
            bc.RegisterParallelCheck<CustomParallelCheck>();
            bc.RegisterHandler<InfoWriterHandler>(ContextEnum.InfoStatusResponce);
            try
            {
                bc.Connect();
                bc.StartPoling();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }


        }
    }
}
