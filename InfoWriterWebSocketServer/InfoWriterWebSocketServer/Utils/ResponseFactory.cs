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
        public static byte[] Hello()
        {
            var messageBytes = ConcateBytes(Encoding.UTF8.GetBytes($"{(int)ContextEnum.Hello}<c>Hello"), new byte[1] { (byte)0 });
            var mm = GetMask(false, messageBytes.Length);
            return ConcateBytes(ConcateBytes(GetHeaderBytes(FrameMessageEnum.Text, true), mm.Bytes), messageBytes);
        }

        public static byte[] Text(string text)
        {
            var messageBytes = ConcateBytes(Encoding.UTF8.GetBytes($"{(int)ContextEnum.Text}<c>{text}"), new byte[1] { (byte)0 });
            var mm = GetMask(false, messageBytes.Length);
            return ConcateBytes(ConcateBytes(GetHeaderBytes(FrameMessageEnum.Text, true), mm.Bytes), messageBytes);
        }

        public static byte[] GetHeaderBytes(FrameMessageEnum frame, bool isFullAnswer)
        {
            var hb = new BitArray(8);
            hb[7] = isFullAnswer;
            var fb = new BitArray(new byte[1] { (byte)frame });
            hb[3] = fb[3];
            hb[2] = fb[2];
            hb[1] = fb[1];
            hb[0] = fb[0];
            var retBytes = new byte[1];
            hb.CopyTo(retBytes, 0);
            return retBytes;
        }

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
            var messageBytes = Encoding.UTF8.GetBytes("ping");
            var mm = GetMask(false, messageBytes.Length);
            return ConcateBytes(ConcateBytes(GetHeaderBytes(FrameMessageEnum.Ping, true), mm.Bytes), messageBytes);
        }

        public static byte[] ConectionClose(string cause)
        {
            var messageBytes = Encoding.UTF8.GetBytes("Conection close: " + cause);
            var mm = GetMask(false, messageBytes.Length);
            return ConcateBytes(ConcateBytes(GetHeaderBytes(FrameMessageEnum.ConectionClose, true), mm.Bytes), messageBytes);
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

        public static MaskModel GetMask(bool haveMask, long payloadLength)
        {
            var bitArr = new bool[8];
            bitArr[7] = haveMask;
            var plBitArr = new BitArray(BitConverter.GetBytes(payloadLength));
            int plCode = 0;
            byte[] lengthBytes = null;
            if (payloadLength < 126)
            {
                plCode = (int)payloadLength;
            }
            else if (payloadLength < 65536)
            {
                plCode = 126;
                var plb = new BitArray(BitConverter.GetBytes(payloadLength));
                var b = new BitArray(16);
                for (int i = 15; i > -1; i--)
                {
                    b[i] = plb[i];
                }
                lengthBytes = new byte[2];
                b.CopyTo(lengthBytes, 0);
            }
            else if (payloadLength >= 65536)
            {
                plCode = 127;
                var plb = new BitArray(BitConverter.GetBytes(payloadLength));
                var b = new BitArray(64);
                for (int i = 63; i > -1; i--)
                {
                    b[i] = plb[i];
                }
                lengthBytes = new byte[8];
                b.CopyTo(lengthBytes, 0);
            }
            var plCodeBitArr = new BitArray(BitConverter.GetBytes(plCode));
            for (int i = 6; i > -1; i--)
            {
                bitArr[i] = plCodeBitArr[i];
            }
            var maskBitArr = new BitArray(bitArr);
            var maskByte = new byte[1];
            maskBitArr.CopyTo(maskByte, 0);
            var mm = new MaskModel
            {
                EncodeBytes = haveMask ? CreateRandom4ByteMask() : null,
                Bytes = lengthBytes != null ? ConcateBytes(maskByte, lengthBytes) : maskByte
            };
            return mm;
        }

        public static byte[] CreateRandom4ByteMask()
        {
            var rand = new Random();
            var retBytes = new byte[4];
            var b = new BitArray(32);
            for (int i = 0; i < 32; i++)
            {
                b[i] = rand.Next(0, 100) > 50 ? true : false;
            }
            b.CopyTo(retBytes, 0);
            return retBytes;
        }

        public static byte[] ConcateBytes(byte[] arr1, byte[] arr2)
        {
            var narr = new byte[arr1.Length + arr2.Length];
            var index = 0;
            for (int i = 0; i < arr1.Length; i++)
            {
                narr[index] = arr1[i];
                index++;
            }
            for (int i = 0; i < arr2.Length; i++)
            {
                narr[index] = arr2[i];
                index++;
            }
            return narr;
        }
        public static byte[] ApplyMask(byte[] arr, byte[] mask)
        {
            var nm = new byte[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                nm[i] = (Byte)(arr[i] ^ mask[i % 4]);
            }
            return nm;
        }
    }



    public class MaskModel
    {
        public byte[] Bytes { get; set; }
        public byte[] EncodeBytes { get; set; }
    }
}
