using InfoWriterWebSocketServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWriterWebSocketServer.CustomUtilities
{
    public class CustomOnShutdown : IShutdown
    {
        public GlobalStorage globalStorage;
        public SessionStorage sessionStorage;

        public CustomOnShutdown(GlobalStorage gs, SessionStorage ss)
        {
            globalStorage = gs;
            sessionStorage = ss;

        }
        public void OnShutdown()
        {
            if (sessionStorage.ComputerName != null)
            {
                if (globalStorage.OnlineDevices.ContainsKey(sessionStorage.ComputerName))
                {
                    globalStorage.OnlineDevices[sessionStorage.ComputerName] = false;
                }
                Console.WriteLine($"OnShutdown ComputerName - {sessionStorage.ComputerName}; Status - {globalStorage.OnlineDevices[sessionStorage.ComputerName]}");
            }
        }
    }
}
