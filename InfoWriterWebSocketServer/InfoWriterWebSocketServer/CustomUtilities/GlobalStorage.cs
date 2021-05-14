using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.CustomUtilities
{
    public class GlobalStorage
    {
        public Dictionary<string, bool> OnlineDevices = new Dictionary<string, bool>();
    }
}
