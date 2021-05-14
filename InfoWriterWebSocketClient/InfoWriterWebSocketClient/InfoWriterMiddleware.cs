using InfoWriterWebSocketClient.Client.Abstractions;
using InfoWriterWebSocketClient.Client.Enums;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace InfoWriterWebSocketClient
{
    public class InfoWriterMiddleware : IMiddleware
    {
        public InfoStorage infoStorage;

        public InfoWriterMiddleware(InfoStorage iS)
        {
            infoStorage = iS;
        }

        public IHandlResult Execute()
        {
            if (infoStorage.lastInfoReportTime + 5 < DateTimeOffset.Now.ToUnixTimeSeconds())
            {
                Console.WriteLine($"lastInfoReportTime {infoStorage.lastInfoReportTime}");
                infoStorage.lastInfoReportTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                Console.WriteLine($"NewInfoReportTime {infoStorage.lastInfoReportTime}");
                var im = new InfoModel
                {
                    osname = Environment.OSVersion.ToString(),
                    context = (int)ContextEnum.Info,
                    dotnetversion = Environment.Version.ToString(),
                    timezone = TimeZone.CurrentTimeZone.ToString(),
                    compname = Environment.MachineName
                };
                return im;
            }
            return null;
        }
    }
}
