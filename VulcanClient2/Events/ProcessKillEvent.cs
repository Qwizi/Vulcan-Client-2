using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;

namespace VulcanClient2.Events
{
    public class ProcessKillEvent : Event
    {
        public ProcessKillEvent(SocketIO socket, string name, Notification notification) : base(socket, name, notification) {}

        public override async Task Run(SocketIOResponse response)
        {
            int processId = response.GetValue(0).Value<int>("processId");
            foreach (var process in Process.GetProcesses().Where(p => p.MainWindowTitle != "").ToArray())
            {
                if (process.Id == processId)
                {
                    process.Kill();
                    Log.Information($"Pomyslnie zamknieto proces {processId}");
                    await Task.Run(() => Notification.Success.Send($"Pomyslnie zamknieto proces {processId}"));
                }
            }
        }
    }
}