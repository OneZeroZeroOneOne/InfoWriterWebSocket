using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer
{
    public class InfoModel
    {
        public int context { get; set; }
        public string osname { get; set; }
        public string timezone { get; set; }
        public string dotnetversion { get; set; }
    }
}
