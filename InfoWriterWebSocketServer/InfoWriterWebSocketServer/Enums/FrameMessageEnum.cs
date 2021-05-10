using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.Enums
{
    public enum FrameMessageEnum : byte
    {
        Text = 0x1,
        Binary = 0x2,
        Ping = 0x9,
        ConectionClose = 0x8,
    }
}
