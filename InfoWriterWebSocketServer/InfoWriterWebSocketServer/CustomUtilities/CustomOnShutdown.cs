using InfoWriterWebSocketServer.CustomModels;
using InfoWriterWebSocketServer.Server.Abstractions;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

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
                if(sessionStorage.infoModel != null)
                {
                    sessionStorage.infoModel.statusid = 2;
                    RequestMaker.Post(Environment.GetEnvironmentVariable("azurefuncurl"), JsonSerializer.Serialize<InfoModel>(sessionStorage.infoModel), "application/json");
                }
                Console.WriteLine($"OnShutdown ComputerName - {sessionStorage.ComputerName}; Status - {globalStorage.OnlineDevices[sessionStorage.ComputerName]}");
            }
        }

        public string Post(string uri, string data, string contentType, string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;
            request.Method = method;

            using (Stream requestBody = request.GetRequestStream())
            {
                requestBody.Write(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
