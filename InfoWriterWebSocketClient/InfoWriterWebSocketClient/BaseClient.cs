using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoWriterWebSocketClient
{
    public class BaseClient
    {
        private ClientWebSocket wsc;
        private Uri uri;
        public IServiceProvider serviceProvider;
        public Dictionary<ContextEnum, Type> handlers = new Dictionary<ContextEnum, Type>();
        public string contextSeparator = "<c>";
        public 

        public BaseClient(Uri u, IServiceProvider sp)
        {
            serviceProvider = sp;
            uri = u;
            wsc = new ClientWebSocket();

        }

        public async Task Connect()
        {
            await wsc.ConnectAsync(uri, CancellationToken.None);
        }

        public void RegisterHandler<T>(ContextEnum ce)
        {
            handlers[ce] = typeof(T);
        }

        public IHanlder GetHandler(ContextEnum ce)
        {
            if (handlers.ContainsKey(ce))
            {
                return (IHanlder)ActivatorUtilities.CreateInstance(serviceProvider, handlers[ce]);
            }
            return null;
        }

        public async Task StartPoling()
        {
            var scope = serviceProvider.CreateScope();
            try
            {
                var messageHello = ResponseFactory.Hello();
                await wsc.SendAsync(messageHello, WebSocketMessageType.Text, true, CancellationToken.None);
                var parser = new UpdateParser();
                while (wsc.State == WebSocketState.Open)
                {
                    var payloadBytes = new byte[1024];
                    var resp = await wsc.ReceiveAsync(payloadBytes, CancellationToken.None);
                    var payload = Encoding.UTF8.GetString(payloadBytes);
                    parser.SetPayload(payload);
                    var update = parser.Parse(contextSeparator);
                    var handler = GetHandler(update.Context);
                    if (handler != null) handler.Handle();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void SetContextSeparatorSec(string sep)
        {
            contextSeparator = sep;
        }
    }
}
