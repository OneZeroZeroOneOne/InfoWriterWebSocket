using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;

namespace InfoWriterWebSocketServer.Utils
{
    public class ByteReader
    {
        public readonly byte[] bytes;
        public long index;
        public ByteReader(byte[] b)
        {
            bytes = b;
            index = 0;
        }

        public BitArray GetBits()
        {
            byte[] ba = new byte[1] { bytes[index++] };
            return new BitArray(ba);
        }

        public string GetText(long length)
        {
            byte[] b = new byte[length];
            for(int i = 0; i<length; i++)
            {
                b[i] = bytes[i + index];
            }
            index += length;
            return Encoding.UTF8.GetString(b);
        }

        public byte GetByte()
        {
            return bytes[index++];
        }

        public byte[] Get8BytesIntPayloadLenght()
        {
            var b = new byte[8];
            for(int i = 0; i<8; i++)
            {
                b[i] = bytes[index++];
            }
            return b;
        }

        public int Get2BytesIntPayloadLenght()
        {
            var nb = new byte[2];
            for(int i = 0; i<2; i++)
            {
                nb[i] = GetByte();
            }
            Array.Reverse(nb);
            var a = ConcateBytes(nb, new byte[2]);
            return BitConverter.ToInt32(a);
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

        public byte[] GetPaylaod(long length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                b[i] = bytes[i + index];
            }
            index += length;
            return b;
        }
    }
}
