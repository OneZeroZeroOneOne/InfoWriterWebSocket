using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.CustomUtilities
{
    public class DemoService
    {
        public int result;
        public int GetRand()
        {
            var r = new Random();
            result = r.Next(0, 100);
            return result;
        }
    }
}
