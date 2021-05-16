using InfoWriterWebSocketServer.CustomModels;
using InfoWriterWebSocketServer.CustomUtilities;
using InfoWriterWebSocketServer.Enums;
using InfoWriterWebSocketServer.Server.Abstractions;
using InfoWriterWebSocketServer.Server.Models;
using System;
using System.Text.Json;

namespace InfoWriterWebSocketServer.CustomHandlers
{
    public class InfoWriterHandler : IHandler
    {
        public UserContext userContext;
        public DemoService demoService;
        public GlobalStorage globalStorage;
        public SessionStorage sessionStorage;
        public InfoWriterHandler(DemoService ds,UserContext uc, GlobalStorage gs, SessionStorage ss)
        {
            userContext = uc;
            demoService = ds;
            globalStorage = gs;
            sessionStorage  = ss;

        }
        public IHandlResult Handle(string model)
        {
            Console.WriteLine($"last rand num - {demoService.result}");
            demoService.GetRand();
            Console.WriteLine($"payload - {userContext.Update.Payload}");
            InfoModel infomodel = JsonSerializer.Deserialize<InfoModel>(model);
            sessionStorage.infoModel = infomodel;
            sessionStorage.ComputerName = infomodel.compname;
            if (globalStorage.OnlineDevices.ContainsKey(sessionStorage.ComputerName))
            {
                Console.WriteLine($"OnlineDevices compname {sessionStorage.ComputerName} status before {globalStorage.OnlineDevices[sessionStorage.ComputerName]}");
            }
            globalStorage.OnlineDevices[sessionStorage.ComputerName] = true;
            Console.WriteLine($"InfoWriterHandler {sessionStorage.ComputerName} set online");

            var res = new InfoHandlResult();
            res.context = ContextEnum.InfoStatusResponce;
            res.status = "Ok";
            return res;
        }
    }
}
