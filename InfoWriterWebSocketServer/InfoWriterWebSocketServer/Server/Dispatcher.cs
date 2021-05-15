using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Server.Models;
using InfoWriterWebSocketServer.Server.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
        public int pingInterval = 2000;

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
            var client = (TcpClient)obj;
            var stream = client.GetStream();
            var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserContext>();
            context.Client = client;
            CancellationTokenSource cts = new CancellationTokenSource();
            context.CancellationThreadToken = cts.Token;
            Thread senderThread = new Thread(new ParameterizedThreadStart(SenderThread));
            senderThread.Start(context);
            Thread pingThread = new Thread(new ParameterizedThreadStart(PingThread));
            pingThread.Start(context);
            try
            {
                var updateParser = new UpdateParser();
                var heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (onSturtup != null)
                {
                    var st = (ISturtup)ActivatorUtilities.CreateInstance(scope.ServiceProvider, onSturtup);
                    var res = st.OnSturtup();
                    if(res != null)
                    {
                        context.QueueMessage.Enqueue(ResponseFactory.Text(res.ToJson()));
                    }
                }
                while (client.Connected)
                {
                    if (DateTimeOffset.Now.ToUnixTimeSeconds() - heartbeatTime > timeoutSec)
                    {
                        Console.WriteLine("the client is not responding. detachment");
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
                            context.Update = update;
                            foreach (var middleware in middlewares)
                            {
                                var middlewareObj = (IMiddleware)ActivatorUtilities.CreateInstance(scope.ServiceProvider, middleware);
                                var res = middlewareObj.Execute();
                                if (res != null)
                                {
                                    context.QueueMessage.Enqueue(ResponseFactory.Text(res.ToJson()));
                                }
                            }
                            if (update.Frame == FrameMessageEnum.Ping)
                            {
                                heartbeatTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                            }
                            else if (update.Frame == FrameMessageEnum.Text)
                            {
                                var data = (JObject)JsonConvert.DeserializeObject(update.Payload);
                                ContextEnum ce = (ContextEnum)(data.ContainsKey("context") ? data["context"].Value<int>() : throw new Exception("context absent"));
                                var handler = GetHandler(ce, scope.ServiceProvider);
                                if (handler != null)
                                {
                                    context.ContextEnum = ce;
                                    var res = handler.Handle(update.Payload);
                                    if (res != null)
                                    {
                                        context.QueueMessage.Enqueue(ResponseFactory.Text(res.ToJson()));
                                    }
                                }
                            }
                            else if (update.Frame == FrameMessageEnum.ConectionClose)
                            {
                                throw new Exception("client close connection");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConectionClose(context, ex.Message);
                Console.WriteLine(ex.Message);
            }
            if (onShutdown != null)
            {
                var st = (IShutdown)ActivatorUtilities.CreateInstance(scope.ServiceProvider, onShutdown);
                st.OnShutdown();
            }
            cts.Cancel();
            stream.Close();
            client.Close();
        }



        public void PingThread(object obj)
        {
            var userContext = (UserContext)obj;
            while (!userContext.CancellationThreadToken.IsCancellationRequested)
            {
                var msg = ResponseFactory.Pong();
                userContext.QueueMessage.Enqueue(msg);
                Console.WriteLine("Add ping");
                Thread.Sleep(pingInterval);
            }
            Console.WriteLine("userContext.CancellationThreadToken.IsCancellationRequested = {0}", userContext.CancellationThreadToken.IsCancellationRequested);

        }

        public void SenderThread(object obj)
        {
            var userContext = (UserContext)obj;
            var stream = userContext.Client.GetStream();
            try
            {
                while (!userContext.CancellationThreadToken.IsCancellationRequested)
                {
                    byte[] msg;
                    if (userContext.QueueMessage.TryDequeue(out msg))
                    {
                        Console.WriteLine($"Send msg - {Encoding.UTF8.GetString(msg)}");
                        stream.Write(msg, 0, msg.Length);
                    }
                }
                Console.WriteLine("userContext.CancellationThreadToken.IsCancellationRequested = {0}", userContext.CancellationThreadToken.IsCancellationRequested);
            }
            catch
            {
            }
        }

        public void ConectionClose(UserContext userContext, string cause)
        {
            var msg = ResponseFactory.ConectionClose(cause);
            userContext.QueueMessage.Enqueue(msg);
        }

        public void SetTimeoutSec(float timeInSecFloat)
        {
            timeoutSec = timeInSecFloat;
        }
    }
}
