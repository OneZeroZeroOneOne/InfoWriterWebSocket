using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Models;
using System;

namespace InfoWriterWebSocketServer.Server
{
    public class UserContext
    {
        public ContextEnum contextEnum;
        public Update Update { get; set; }
        //public Guid Id { get; set; }
    }
}
