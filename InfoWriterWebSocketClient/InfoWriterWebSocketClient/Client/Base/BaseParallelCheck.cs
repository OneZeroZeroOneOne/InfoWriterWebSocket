using InfoWriterWebSocketClient.Client.Abstractions;
using System.Threading;
using System;

namespace InfoWriterWebSocketClient.Client.Base
{
    public class BaseParallelCheck : IParallelCheck
    {
        public CancellationToken cancellationThreadToken;
        public BaseParallelCheck(CancellationToken ctt)
        {
            cancellationThreadToken = ctt;
        }

        public void StartIteartion()
        {
            Console.WriteLine("StartIteartion");
            while (!cancellationThreadToken.IsCancellationRequested)
            {
                Check();
            }
        }

        public virtual void Check()
        {
        }
    }
}
