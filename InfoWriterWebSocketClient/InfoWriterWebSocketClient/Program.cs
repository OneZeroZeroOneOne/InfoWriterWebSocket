using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace InfoWriterWebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var messageHello = ResponseFactory.Hello();

            Console.ReadLine();
            //var b = new byte[];
            /*await wsc.SendAsync();

            while (wsc.State == WebSocketState.Open)
            {
                Console.Write("Input message ('exit' to exit): ");
                string msg = Console.ReadLine();
                if (msg == "exit")
                {
                    break;
                }
            }
            */
        }
    }
}
