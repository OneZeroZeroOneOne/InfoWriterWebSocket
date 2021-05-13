using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoWriterWebSocketClient
{
    public class UpdateParser
    {
        string _payload;
        public void SetPayload(string payload)
        {
            _payload = payload;
        }
        public Update Parse()
        {
            string[] parr = _payload.Split("<c>");
            return new Update
            {
                Context = (ContextEnum)int.Parse(parr.First()),
                Payload = parr.Last()
            };
        } 
    }
}
