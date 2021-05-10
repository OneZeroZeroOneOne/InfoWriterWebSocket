using InfoWriterWebSocketServer.Enums;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoWriterWebSocketServer.Utils
{
    public class ResponseFactory
    {
        public static byte[] Handshake(string data)
        {
            const string eol = "\r\n";
            byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                + "Connection: Upgrade" + eol
                + "Upgrade: websocket" + eol
                + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                    SHA1.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(
                            new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                        )
                    )
                ) + eol
                + eol);
            return response;
        }

        public static byte[] Ping()
        {
            var ret = new byte[1] { GetHeaderByte(FrameMessageEnum.Ping, true)};
            return ret;
        }

        public static byte GetHeaderByte(FrameMessageEnum frame, bool isFullAnswer)
        {
            var hb = new BitArray(8);
            hb[0] = isFullAnswer;
            var fb = new BitArray(new byte[1] { (byte)frame });
            hb[4] = fb[4];
            hb[5] = fb[5];
            hb[6] = fb[6];
            hb[7] = fb[7];
            var retBytes = new byte[1];
            hb.CopyTo(retBytes, 0);
            return retBytes[0];
        }



    }
}
