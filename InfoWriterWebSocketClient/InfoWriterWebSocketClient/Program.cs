using System;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;

namespace InfoWriterWebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var iwc = new InfoWriterClient(new Uri("ws://127.0.0.1:7776"));
            await iwc.Connect();
            await iwc.StartPoling();
        }
    }
}
