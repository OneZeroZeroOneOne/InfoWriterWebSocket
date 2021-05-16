using InfoWriterWebSocketClient.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace InfoWriterWebSocketClient
{
    public class InfoModel : IHandlResult
    {
        public int context { get; set; }
        public string osname { get; set; }
        public string timezone { get; set; }
        public string dotnetversion { get; set; }
        public string compname { get; set; }


        public string ToJson()
        {
            string json = JsonSerializer.Serialize<InfoModel>(this);
            return json;
        }
    }
}
