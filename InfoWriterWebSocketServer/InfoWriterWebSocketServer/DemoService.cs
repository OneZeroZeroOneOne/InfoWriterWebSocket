using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer
{
    public class DemoService
    {
        public int GetRand()
        {
            var r = new Random();
            return r.Next(0, 100);
        }
    }
}
