using InfoWriterWebSocketClient.Client.Abstractions;
using InfoWriterWebSocketClient.Client.Base;
using InfoWriterWebSocketClient.Client.Enums;
using InfoWriterWebSocketClient.Client.Extentions;
using InfoWriterWebSocketClient.Client.Models;
using InfoWriterWebSocketClient.Client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
        public List<Type> parallelChecks = new List<Type>();
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

        public void RegisterParallelCheck<T>()
        {
            parallelChecks.Add(typeof(T));
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
            var context = serviceProvider.GetRequiredService<UserContext>();
            context.Client = client;
            CancellationTokenSource cts = new CancellationTokenSource();
            context.CancellationThreadToken = cts.Token;
            Thread senderThread = new Thread(new ParameterizedThreadStart(SenderThread));
            senderThread.Start(context);
            foreach (var parallelCheckType in parallelChecks)
            {
                var parallelCheck = (IParallelCheck)ActivatorUtilities.CreateInstance(serviceProvider, parallelCheckType, context.CancellationThreadToken);
                Thread parallelCheckThread = new Thread(new ThreadStart(parallelCheck.StartIteartion));
                parallelCheckThread.Start();
            }
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
                            context.Update = update;
                            foreach (var middleware in middlewares)
                            {
                                var middlewareObj = (IMiddleware)ActivatorUtilities.CreateInstance(serviceProvider, middleware);
                                var res = middlewareObj.Execute();
                                if(res != null)
                                {
                                    context.QueueMessage.Enqueue(ResponseFactory.Text(res.ToJson()));
                                }
                            }
                            if (update.Frame == FrameMessageEnum.Text)
                            {
                                var data = (JObject)JsonConvert.DeserializeObject(update.Payload);
                                ContextEnum ce = (ContextEnum)(data.ContainsKey("context") ? data["context"].Value<int>() : throw new Exception("context absent"));
                                var handler = GetHandler(ce);
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
                            else if (update.Frame == FrameMessageEnum.Pong)
                            {
                                context.QueueMessage.Enqueue(ResponseFactory.Ping());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            cts.Cancel();

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
    }
}
