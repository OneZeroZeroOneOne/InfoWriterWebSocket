using InfoWriterWebSocketServer.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.Models
{
    public class Update
    {
        public FrameMessageEnum  Frame { get; set; }
        public string Payload { get; set; }
        public long PayloadLength { get; set; }
        public byte[] Mask { get; set; } 
        public bool IsFullUpdate { get; set; }
    }
}
