using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketClient
{
    public class Update
    {
        public string Payload { get; set; }
        public ContextEnum Context { get; set; }
    }
}
