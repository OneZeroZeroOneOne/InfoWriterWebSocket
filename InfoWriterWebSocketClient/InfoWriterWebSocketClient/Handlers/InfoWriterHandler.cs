using InfoWriterWebSocketClient.Client.Abstractions;
using InfoWriterWebSocketClient.Client.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace InfoWriterWebSocketClient.Handlers
{
    public class InfoWriterHandler : IHanlder
    {
        public InfoWriterHandler(InfoStorage storage)
        {
            infoStorage = storage;
        }

        public InfoStorage infoStorage;
        public IHandlResult Handle(string json)
        {
            var infoStatusResponseModel = JsonSerializer.Deserialize<InfoStatusResponseModel>(json);
            if(infoStatusResponseModel.status == "Ok") infoStorage.lastInfoReportTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            return null;
        }
    }
}
