using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;

namespace InfoWriterWebSocketServer.Utils
{
    public class ByteReader
    {
        private byte[] _bytes;
        private int _index;
        public ByteReader(byte[] bytes)
        {
            _bytes = bytes;
            _index = 0;
        }

        public BitArray GetBits()
        {
            byte[] ba = new byte[1] { _bytes[_index++] };
            return new BitArray(ba);
        }

        public string GetText()
        {
            byte[] b = new byte[1024];
            _bytes.Skip(_index).ToArray().CopyTo(b, 0);
            return Encoding.UTF8.GetString(b);
        }

        public string GetText(byte[] mask)
        {
            var enc = _bytes.Skip(_index).ToArray();
            byte[] b = new byte[enc.Length];
            for (int i = 0; i < enc.Length; i++)
            {
                b[i] = (Byte)(enc[i] ^ mask[i % 4]);
            }
            return Encoding.UTF8.GetString(b);
        }

        public byte GetByte()
        {
            return _bytes[_index++];
        }

        public byte[] Get8Bytes()
        {
            var b = new byte[8];
            for(int i = 0; i<8; i++)
            {
                b[i] = _bytes[_index++];
            }
            return b;
        }

        public int Get2ByteIntPayloadLenght()
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
    }
}
