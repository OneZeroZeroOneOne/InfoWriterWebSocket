using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Utils;
using System;
using System.Text.Json;

namespace InfoWriterWebSocketServer
{
    public class InfoWriterHandler : IHandler
    {
        public UserContext userContext;
        public DemoService demoService;
        public InfoWriterHandler(DemoService ds,UserContext uc)
        {
            userContext = uc;
            demoService = ds;
        }
        public IHandlResult Handle(string model)
        {
            var new1 = new InfoModel();
            new1.context = 2;
            new1.dotnetversion = "123";
            Console.WriteLine($"last rand num - {demoService.result}");
            demoService.GetRand();
            Console.WriteLine($"payload - {userContext.Update.Payload}");
            InfoModel infomodel = JsonSerializer.Deserialize<InfoModel>(model);
            
            var res = new InfoHandlResult();
            res.context = ContextEnum.InfoStatusResponce;
            res.status = "Ok";
            return res;
        }
    }
}
