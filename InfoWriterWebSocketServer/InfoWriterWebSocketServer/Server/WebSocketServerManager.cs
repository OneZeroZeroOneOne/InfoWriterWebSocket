using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InfoWriterWebSocketServer.Server
{
    public class WebSocketServerManager
    {
        readonly string _host;
        private List<BaseService> _servicesList;
        public WebSocketServerManager(string host)
        {
            _host = host;
            _servicesList = new List<BaseService>();
        }

        public void AddWebSocketService<T>(int port) where T : BaseService, new()
        {
            T bs = new T();
            bs.SetHostPort(_host, port);
            _servicesList.Add(bs);
        }

        public void Start()
        {
            foreach(var i in _servicesList)
            {
                Thread myThread = new Thread(new ThreadStart(i.StartListen));
                myThread.Start();
            }
        }


    }
}
