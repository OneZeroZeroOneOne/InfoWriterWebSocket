using InfoWriterWebSocketClient.Client.Base;
using InfoWriterWebSocketClient.Client.Enums;
using InfoWriterWebSocketClient.Client.Models;
using InfoWriterWebSocketClient.Client.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InfoWriterWebSocketClient.ParallelChecks
{
    public class CustomParallelCheck : BaseParallelCheck
    {
        public UserContext userContext;
        public InfoStorage infoStorage;
        public CustomParallelCheck(CancellationToken ctt, UserContext uc, InfoStorage iS) : base(ctt)
        {
            userContext = uc;
            infoStorage = iS;
        }

        public override void Check()
        {
            if(infoStorage.lastInfoReportTime + 5 < DateTimeOffset.Now.ToUnixTimeSeconds())
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
                userContext.QueueMessage.Enqueue(ResponseFactory.Text(im.ToJson()));
            }
        }
    }
}
