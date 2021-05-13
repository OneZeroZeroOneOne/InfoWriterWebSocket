using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoWriterWebSocketClient
{
    public class InfoWriterClient
    {
        private ClientWebSocket _wsc;
        private Uri _uri;

        public InfoWriterClient(Uri uri)
        {
            _uri = uri;
            _wsc = new ClientWebSocket();
           
        }

        public async Task Connect()
        {
            await _wsc.ConnectAsync(_uri, CancellationToken.None);
        }

        public async Task StartPoling()
        {
            try
            {
                var messageHello = ResponseFactory.Hello();
                await _wsc.SendAsync(messageHello, WebSocketMessageType.Text, true, CancellationToken.None);
                var parser = new UpdateParser();
                while (_wsc.State == WebSocketState.Open)
                {
                    var payloadBytes = new byte[1024];
                    var resp = await _wsc.ReceiveAsync(payloadBytes, CancellationToken.None);
                    var payload = Encoding.UTF8.GetString(payloadBytes);
                    parser.SetPayload(payload);
                    var update = parser.Parse();
                    if (update.Context == ContextEnum.Hello)
                    {
                        Console.WriteLine($"Server answered: {update.Payload}");
                        messageHello = ResponseFactory.Hello();
                        await _wsc.SendAsync(messageHello, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

    }
}
