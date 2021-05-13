using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.Extentions
{
    public static class ByteArrayExt
    {
        public static void ApplyMask(this byte[] enc, byte[] mask)
        {
            for (int i = 0; i < enc.Length; i++)
            {
                enc[i] = (Byte)(enc[i] ^ mask[i % 4]);
            }
        }
    }
}
