using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketClient
{
    public static class ResponseFactory
    {
        public static byte[] Hello()
        {
            var messageBytes = ConcateBytes(Encoding.UTF8.GetBytes($"{(int)ContextEnum.Hello}<c>Hello"), new byte[1] { (byte)0 });
            return messageBytes;
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
