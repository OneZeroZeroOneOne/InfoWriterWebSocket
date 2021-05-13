using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace InfoWriterWebSocketServer.Server
{
    public class BaseDispatcher
    {
        public Dictionary<ContextEnum, Type> handlers = new Dictionary<ContextEnum, Type>();
        private IServiceProvider serviceProvider;
        public float timeoutSec = 5;

        public BaseDispatcher( IServiceProvider sp)
        {
            serviceProvider = sp;
        }

        public void AddHandler<T>(ContextEnum ce) where T : IHandler
        {
            handlers[ce] = typeof(T);
        }

        public IHandler GetHandler(ContextEnum ce, IServiceProvider serviceProvider)
        {
            if (handlers.ContainsKey(ce))
            {
                return (IHandler)ActivatorUtilities.CreateInstance(serviceProvider, handlers[ce]);
            }
            return null;
        }

        public void StartPolling(object obj)
        {
            var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserContext>();
            var client = (TcpClient)obj;
            var stream = client.GetStream();
            try
            {
                var updateParser = new UpdateParser();
                var heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                Pong(client);
                while (client.Connected)
                {
                    if (DateTimeOffset.Now.ToUnixTimeSeconds() - heartbeatTime > timeoutSec)
                    {
                        Console.WriteLine("the client is not responding. detachment");
                        ConectionClose(client, "heartbeat stopped");
                        throw new Exception("heartbeat stopped");
                    }
                    Pong(client);
                    if (client.Available > 0)
                    {
                        var available = client.Available;
                        byte[] bytes = new byte[available];
                        stream.Read(bytes, 0, available);
                        updateParser.SetBytes(bytes);
                        var updates = updateParser.Parse();
                        if(updates.Count >= 2)
                        {
                            Console.WriteLine($"updates.Count = {updates.Count}");
                        }
                        foreach(var update in updates)
                        {
                            Console.WriteLine($"update frame - {update.Frame}");
                            if (update.Frame == FrameMessageEnum.Pong)
                            {
                                heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                            }
                            else if (update.Frame == FrameMessageEnum.Text)
                            {
                                var data = (JObject)JsonConvert.DeserializeObject(update.Payload);
                                ContextEnum ce = (ContextEnum)(data.ContainsKey("context") ? data["context"].Value<int>() : throw new Exception("context absent"));
                                context.Update = update;
                                var handler = GetHandler(ce, scope.ServiceProvider);
                                if (handler != null)
                                {
                                    context.contextEnum = ce;
                                    var res = handler.Handle();
                                }
                            }
                            else if (update.Frame == FrameMessageEnum.ConectionClose)
                            {
                                ConectionClose(client, "client close connection");
                                throw new Exception("client close connection");
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }
            stream.Close();
            client.Close();
        }

        public void Pong(TcpClient client)
        {
            Console.WriteLine("Pong");
            var stream = client.GetStream();
            var msg = ResponseFactory.Ping();
            stream.Write(msg, 0, msg.Length);
        }
        public void ConectionClose(TcpClient client, string cause)
        {
            var stream = client.GetStream();
            var msg = ResponseFactory.ConectionClose(cause);
            stream.Write(msg, 0, msg.Length);
        }

        public void SetTimeoutSec(float timeInSecFloat)
        {
            timeoutSec = timeInSecFloat;
        }
    }
}
