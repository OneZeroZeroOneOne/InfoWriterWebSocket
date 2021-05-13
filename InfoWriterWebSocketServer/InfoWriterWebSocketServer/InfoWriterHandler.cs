using InfoWriterWebSocketServer.Server;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Utils;
using System;

namespace InfoWriterWebSocketServer
{
    public class InfoWriterHandler : IHandler
    {
        public UserContext userContext;
        public InfoWriterHandler(UserContext uc)
        {
            userContext = uc;
        }
        public void Handle()
        {
            Console.WriteLine(userContext.Update.Payload);
            var stream = userContext.client.GetStream();
            stream.Write(ResponseFactory.Text(userContext.Update.Payload));
        }
    }
}
