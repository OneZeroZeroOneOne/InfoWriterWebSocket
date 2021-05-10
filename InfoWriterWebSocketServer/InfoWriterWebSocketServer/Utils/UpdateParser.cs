using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;
using System;
using System.Collections;

namespace InfoWriterWebSocketServer.Utils
{

    public class UpdateParser
    {
        ByteReader byteReader;

        public void SetBytes(byte[] memory)
        {
            byteReader = new ByteReader(memory);
        }

        public Update Parse()
        {
            var hb = byteReader.GetBits();
            var frameTypeBitArr = new bool[] { hb[0], hb[1], hb[2], hb[3], false, false, false, false };
            var fba = new BitArray(frameTypeBitArr);
            byte[] nfba = new byte[1];
            fba.CopyTo(nfba, 0);
            var u = new Update
            {
                Frame = (FrameMessageEnum)nfba[0],
            };
            u.IsFullUpdate = hb[7];
            var maskBits = byteReader.GetBits();
            var payloadLenghtBitArr = new bool[] { maskBits[0], maskBits[1], maskBits[2], maskBits[3], maskBits[4], maskBits[5], maskBits[6], false};
            var mba = new BitArray(payloadLenghtBitArr);
            byte[] nmba = new byte[1];
            mba.CopyTo(nmba, 0);
            u.PayloadLenght = Convert.ToInt32(nmba[0]);
            if (maskBits[7])
            {
                u.Mask = new byte[4] { byteReader.GetByte(), byteReader.GetByte(), byteReader.GetByte(), byteReader.GetByte() };
            }
            u.Context = (ContextEnum)byteReader.GetByte();
            if (u.Frame == FrameMessageEnum.Text)
            {
                if (u.Mask != null)
                {
                    u.Payload = byteReader.GetText(u.Mask);
                }
                else
                {
                    u.Payload = byteReader.GetText();
                }
                
            }
            return u;
             
        }

    }
}
