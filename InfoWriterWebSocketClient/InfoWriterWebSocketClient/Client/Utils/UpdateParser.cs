using InfoWriterWebSocketClient.Client.Enums;
using InfoWriterWebSocketClient.Client.Extentions;
using InfoWriterWebSocketClient.Client.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoWriterWebSocketClient.Client.Utils
{
    public class UpdateParser
    {
        ByteReader byteReader;

        public void SetBytes(byte[] memory)
        {
            byteReader = new ByteReader(memory);
        }

        public List<Update> Parse()
        {
            var retList = new List<Update>();
            while (byteReader.bytes.Length > byteReader.index)
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
                var payloadLenghtBitArr = new bool[] { maskBits[0], maskBits[1], maskBits[2], maskBits[3], maskBits[4], maskBits[5], maskBits[6], false };
                var pba = new BitArray(payloadLenghtBitArr);
                byte[] npba = new byte[1];
                pba.CopyTo(npba, 0);
                if (Convert.ToInt32(npba[0]) < 125)
                {
                    u.PayloadLength = Convert.ToInt32(npba[0]);
                }
                else if (Convert.ToInt32(npba[0]) == 126)
                {
                    u.PayloadLength = byteReader.Get2BytesIntPayloadLenght();
                }
                else if (Convert.ToInt32(npba[0]) == 127)
                {
                    u.PayloadLength = Convert.ToInt32(byteReader.Get8BytesIntPayloadLenght());
                }
                if (maskBits[7])
                {
                    u.Mask = new byte[4] { byteReader.GetByte(), byteReader.GetByte(), byteReader.GetByte(), byteReader.GetByte() };
                }
                byte[] bytePayload = byteReader.GetPaylaod(u.PayloadLength);
                if (u.Frame == FrameMessageEnum.Text)
                {
                    if (u.Mask != null)
                    {
                        bytePayload.ApplyMask(u.Mask);
                        u.Payload = Encoding.UTF8.GetString(bytePayload);
                    }
                    else
                    {
                        u.Payload = Encoding.UTF8.GetString(bytePayload);
                    }
                }
                retList.Add(u);
            }
            return retList;
        }
    }
}
