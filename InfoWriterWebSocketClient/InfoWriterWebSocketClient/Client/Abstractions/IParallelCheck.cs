using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketClient.Client.Abstractions
{
    public interface IParallelCheck
    {
        public void StartIteartion();
        public void Check();
    }
}
