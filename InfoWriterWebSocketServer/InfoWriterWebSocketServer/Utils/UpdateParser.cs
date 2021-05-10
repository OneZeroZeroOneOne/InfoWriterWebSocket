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
            var pba = new BitArray(payloadLenghtBitArr);
            byte[] npba = new byte[1];
            pba.CopyTo(npba, 0);
            if(Convert.ToInt32(npba[0]) < 125)
            {
                u.PayloadLength = Convert.ToInt32(npba[0]);
            }
            else if (Convert.ToInt32(npba[0]) == 126)
            {
                u.PayloadLength = Convert.ToInt32(new byte[2] { byteReader.GetByte(), byteReader.GetByte() });
            }
            else if (Convert.ToInt32(npba[0]) == 127)
            {
                u.PayloadLength = Convert.ToInt32(byteReader.Get8Bytes());
            }
            if (maskBits[7])
            {
                u.Mask = new byte[4] { byteReader.GetByte(), byteReader.GetByte(), byteReader.GetByte(), byteReader.GetByte() };
            }
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
