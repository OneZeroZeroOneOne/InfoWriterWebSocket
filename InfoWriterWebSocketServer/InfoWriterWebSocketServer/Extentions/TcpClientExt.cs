using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoWriterWebSocketServer.Extentions
{
    public static class TcpClientExt
    {
        public static void RFC6455Handshake(this TcpClient client)
        {
            bool IsAccept = false;
            var stream = client.GetStream();
            while (IsAccept == false)
            {
                if (client.Available > 0)
                {
                    byte[] bytes = new byte[client.Available];
                    stream.Read(bytes, 0, client.Available);
                    string s = Encoding.UTF8.GetString(bytes);
                    if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                    {
                        string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                        byte[] response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 101 Switching Protocols\r\n" +
                            "Connection: Upgrade\r\n" +
                            "Upgrade: websocket\r\n" +
                            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");
                        stream.Write(response, 0, response.Length);
                        IsAccept = true;
                    }
                }
                // wait for enough bytes to be available
            }
        }
    }
}
