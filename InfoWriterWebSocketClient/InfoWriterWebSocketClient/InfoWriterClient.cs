using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoWriterWebSocketClient
{
    public class InfoWriterClient : BaseClient
    {
        public InfoWriterClient(Uri u, IServiceProvider sp) : base(u, sp)
        {

        }

    }
}
