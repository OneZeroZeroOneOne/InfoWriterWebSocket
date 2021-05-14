using InfoWriterWebSocketClient.Client.Abstractions;
using InfoWriterWebSocketClient.Client.Enums;
using InfoWriterWebSocketClient.Client.Extentions;
using InfoWriterWebSocketClient.Client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace InfoWriterWebSocketClient.Client
{
    public class BaseClient
    {
        private TcpClient client;
        public string host;
        public int port;
        public IServiceProvider serviceProvider;
        public Dictionary<ContextEnum, Type> handlers = new Dictionary<ContextEnum, Type>();
        public List<Type> middlewares = new List<Type>();
        public int timeout = 10;
        public BaseClient(string h, int p, IServiceProvider sp)
        {
            serviceProvider = sp;
            host = h;
            port = p;
        }

        public void Connect()
        {
            client = new TcpClient(host, port);
            client.RFC6455Handshake(host, port, timeout);
        }

        public void RegisterHandler<T>(ContextEnum ce) where T : IHanlder
        {
            handlers[ce] = typeof(T);
        }

        public void RegisterMiddleware<T>()
        {
            middlewares.Add(typeof(T));
        }

        public IHanlder GetHandler(ContextEnum ce)
        {
            if (handlers.ContainsKey(ce))
            {
                return (IHanlder)ActivatorUtilities.CreateInstance(serviceProvider, handlers[ce]);
            }
            return null;
        }

        public void StartPoling()
        {
            var stream = client.GetStream();
            Console.WriteLine("Start polling");
            try
            {
                var updateParser = new UpdateParser();
                while (client.Connected)
                {
                    if (client.Available > 0)
                    {
                        var available = client.Available;
                        byte[] bytes = new byte[available];
                        stream.Read(bytes, 0, available);
                        updateParser.SetBytes(bytes);
                        var updates = updateParser.Parse();
                        foreach (var update in updates)
                        {
                            foreach (var middleware in middlewares)
                            {
                                var middlewareObj = (IMiddleware)ActivatorUtilities.CreateInstance(serviceProvider, middleware);
                                var res = middlewareObj.Execute();
                                if(res != null)
                                {
                                    stream.Write(ResponseFactory.Text(res.ToJson()));
                                }
                            }
                            if (update.Frame == FrameMessageEnum.Text)
                            {
                                var data = (JObject)JsonConvert.DeserializeObject(update.Payload);
                                ContextEnum ce = (ContextEnum)(data.ContainsKey("context") ? data["context"].Value<int>() : throw new Exception("context absent"));
                                var handler = GetHandler(ce);
                                if (handler != null)
                                {
                                    var res = handler.Handle(update.Payload);
                                    if (res != null)
                                    {
                                        stream.Write(ResponseFactory.Text(res.ToJson()));
                                    }
                                }
                            }
                            else if (update.Frame == FrameMessageEnum.Pong)
                            {
                                Ping();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void Ping()
        {
            var stream = client.GetStream();
            var msg = ResponseFactory.Ping();
            stream.Write(msg, 0, msg.Length);
        }
    }
}
