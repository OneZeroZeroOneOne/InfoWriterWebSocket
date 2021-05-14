using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace InfoWriterWebSocketClient.Client.Extentions
{
    public static class TcpClientExt
    {
        public static void RFC6455Handshake(this TcpClient client, string host, int port, int timeoutSec)
        {
            bool IsAccept = false;
            var stream = client.GetStream();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            var key = new string(Enumerable.Repeat(chars, 22).Select(s => s[random.Next(s.Length)]).ToArray()) + "==";
            string handshakeMsg = "GET /chat HTTP/1.1\r\n" +
                                  $"Host: :{host}:{port}\r\n" +
                                  "Upgrade: websocket\r\n" +
                                  $"Sec-WebSocket-Key: {key}\r\n" +
                                  "Sec-WebSocket-Version: 13\r\n\r\n";
            byte[] handshakeBytes = Encoding.UTF8.GetBytes(handshakeMsg);
            stream.Write(handshakeBytes, 0, handshakeBytes.Length);
            string swka = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);
            Console.WriteLine($"self hash - {swkaSha1Base64}");
            var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            while (IsAccept == false)
            {
                if(startTime + timeoutSec < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    throw new Exception("timeout heandshake");
                }
                if (client.Available > 0)
                {
                    byte[] bytes = new byte[client.Available];
                    stream.Read(bytes, 0, client.Available);
                    string s = Encoding.UTF8.GetString(bytes);
                    if (Regex.IsMatch(s, "^HTTP", RegexOptions.IgnoreCase))
                    {
                        string swkResponce = Regex.Match(s, "Sec-WebSocket-Accept: (.*)").Groups[1].Value.Trim();
                        Console.WriteLine($"responce hash - {swkResponce}");
                        if(swkResponce == swkaSha1Base64)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("hashes do not match");
                        }
                    }
                }
            }
        }
    }
}
