using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketClient.Client.Abstractions
{
    public interface IHanlder
    {
        public IHandlResult Handle(string json);
    }
}
