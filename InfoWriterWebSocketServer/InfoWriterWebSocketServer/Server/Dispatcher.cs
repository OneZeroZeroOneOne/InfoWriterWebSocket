using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Server.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace InfoWriterWebSocketServer.Server
{
    public class Dispatcher
    {
        public Dictionary<ContextEnum, Type> handlers = new Dictionary<ContextEnum, Type>();
        public Dictionary<ContextEnum, Type> handlerModels = new Dictionary<ContextEnum, Type>();
        public List<Type> middlewares = new List<Type>();
        public Type onSturtup;
        public Type onShutdown;
        private IServiceProvider serviceProvider;
        public float timeoutSec = 10;

        public Dispatcher(IServiceProvider sp)
        {
            serviceProvider = sp;
        }

        public void AddHandler<THandler, TModel>(ContextEnum ce) where THandler : IHandler
        {
            handlers[ce] = typeof(THandler);
            handlerModels[ce] = typeof(TModel);
        }

        public void AddMiddleware<T>()
        {
            middlewares.Add(typeof(T));
        }

        public IHandler GetHandler(ContextEnum ce, IServiceProvider serviceProvider)
        {
            if (handlers.ContainsKey(ce))
            {
                var t = handlerModels[ce];
                return (IHandler)ActivatorUtilities.CreateInstance(serviceProvider, handlers[ce]);
            }
            return null;
        }

        public Type GetHandlerModelType(ContextEnum ce)
        {
            if (handlerModels.ContainsKey(ce))
            {
                return handlerModels[ce];
            }
            return null;
        }

        public void OnSturtup<TSturtup>()
        {
            onSturtup = typeof(TSturtup);
        }

        public void OnShutdown<TSturtup>()
        {
            onShutdown = typeof(TSturtup);
        }


        public void StartPolling(object obj)
        {
            Console.WriteLine($"New connection guid {Guid.NewGuid()}");
            var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserContext>();
            var client = (TcpClient)obj;
            var stream = client.GetStream();
            try
            {
                var updateParser = new UpdateParser();
                var heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                Console.WriteLine("Send first pong");
                Pong(client);
                Console.WriteLine("after send first pong");
                if (onSturtup != null)
                {
                    var st = (ISturtup)ActivatorUtilities.CreateInstance(scope.ServiceProvider, onSturtup);
                    var res = st.OnSturtup();
                    if(res != null)
                    {
                        stream.Write(ResponseFactory.Text(res.ToJson()));
                    }
                }
                while (client.Connected)
                {
                    if (DateTimeOffset.Now.ToUnixTimeSeconds() - heartbeatTime > timeoutSec)
                    {
                        Console.WriteLine("the client is not responding. detachment");
                        ConectionClose(client, "heartbeat stopped");
                        throw new Exception("heartbeat stopped");
                    }
                    if (client.Available > 0)
                    {
                        var available = client.Available;
                        byte[] bytes = new byte[available];
                        stream.Read(bytes, 0, available);
                        updateParser.SetBytes(bytes);
                        var updates = updateParser.Parse();
                        foreach(var update in updates)
                        {
                            foreach (var middleware in middlewares)
                            {
                                var middlewareObj = (IMiddleware)ActivatorUtilities.CreateInstance(scope.ServiceProvider, middleware);
                                var res = middlewareObj.Execute();
                                if (res != null)
                                {
                                    stream.Write(ResponseFactory.Text(res.ToJson()));
                                }
                            }
                            if (update.Frame == FrameMessageEnum.Ping)
                            {
                                heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                                Pong(client);
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
                                    var res = handler.Handle(update.Payload);
                                    if (res != null)
                                    {
                                        stream.Write(ResponseFactory.Text(res.ToJson()));
                                    }
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
            if (onShutdown != null)
            {
                var st = (IShutdown)ActivatorUtilities.CreateInstance(scope.ServiceProvider, onShutdown);
                st.OnShutdown();
            }
            stream.Close();
            client.Close();
        }

        public void Pong(TcpClient client)
        {
            var stream = client.GetStream();
            var msg = ResponseFactory.Pong();
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
