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
        public Update Parse(string separator)
        {
            string[] parr = _payload.Split(separator);
            return new Update
            {
                Context = (ContextEnum)int.Parse(parr.First()),
                Payload = parr.Last()
            };
        } 
    }
}
