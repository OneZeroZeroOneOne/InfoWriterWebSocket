using InfoWriterWebSocketServer.Server;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Utils;
using System;

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
        public void Handle()
        {
            Console.WriteLine($"{demoService.result}");
            demoService.GetRand();
            
            Console.WriteLine(userContext.Update.Payload);
            var stream = userContext.client.GetStream();
            stream.Write(ResponseFactory.Text(userContext.Update.Payload));
        }
    }
}
