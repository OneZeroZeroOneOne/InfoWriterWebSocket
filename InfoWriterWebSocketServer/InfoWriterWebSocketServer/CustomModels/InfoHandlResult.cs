using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace InfoWriterWebSocketServer.CustomModels
{
    public class InfoHandlResult : IHandlResult
    {
        public ContextEnum context { get; set; }
        public string status { get; set; }
        public string ToJson()
        {
            string json = JsonSerializer.Serialize<InfoHandlResult>(this);
            return json;
        }
    }
}
