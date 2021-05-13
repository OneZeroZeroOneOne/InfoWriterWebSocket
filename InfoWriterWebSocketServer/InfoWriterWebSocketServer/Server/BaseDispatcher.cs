using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Utils;
using Microsoft.Extensions.DependencyInjection;
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
        private IServiceProvider _serviceProvider;
        public float _timeoutSec = 5;
        public string _contextSeparator = "<c>";

        public BaseDispatcher( IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserContext>();
            context.client = (TcpClient)obj;
            var stream = context.client.GetStream();
            /*try
            {*/
                var updateParser = new UpdateParser();
                var heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                Pong(context.client);
                while (true)
                {
                    if (DateTimeOffset.Now.ToUnixTimeSeconds() - heartbeatTime > _timeoutSec)
                    {
                        Console.WriteLine("the client is not responding. detachment");
                        ConectionClose(context.client, "heartbeat stopped");
                        stream.Close();
                        context.client.Close();
                        break;
                    }
                    Pong(context.client);
                    if (context.client.Available > 0)
                    {
                        var available = context.client.Available;
                        byte[] bytes = new byte[available];
                        stream.Read(bytes, 0, available);
                        updateParser.SetBytes(bytes);
                        var update = updateParser.Parse();
                        Console.WriteLine($"update frame - {update.Frame}");
                        if (update.Frame == FrameMessageEnum.Pong)
                        {
                            heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                        else if (update.Frame == FrameMessageEnum.Text)
                        {
                            string[] parr = update.Payload.Split(_contextSeparator);
                            if (parr.Length < 2) throw new Exception("context is absent");
                            ContextEnum ce = (ContextEnum)int.Parse(parr.First());
                            update.Payload = parr.Last();
                            context.Update = update;
                            var handler = GetHandler(ce, scope.ServiceProvider);
                            handler.Handle();
                        }
                        else if (update.Frame == FrameMessageEnum.ConectionClose)
                        {
                            ConectionClose(context.client, "client close connection");
                            break;
                        }
                    }
                }
            /*}
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
                ConectionClose(context.client, $"error: {ex.Message}");
                stream.Close();
                context.client.Close();
            }*/
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
            _timeoutSec = timeInSecFloat;
        }

        public void SetContextSeparatorSec(string sep)
        {
            _contextSeparator = sep;
        }
    }
}
