using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;
using System.ComponentModel;

namespace VulcanClient2.Events
{
    public class ProcessStartEvent : Event
    {
        public ProcessStartEvent(SocketIO socket, string name, Notification notification) : base(socket, name, notification) {}

        public override async Task Run(SocketIOResponse response)
        {
            string processName = response.GetValue(0).Value<string>("processName");
            var process = new Process();
            process.StartInfo.FileName = processName;
            try
            {
                process.Start();
                Log.Information($"Pomyslnie uruchomino {processName}");
                await Task.Run(() => Notification.Success.Send($"Pomyslnie uruchomino {processName}"));
            }
            catch (Win32Exception e)
            {
                Log.Fatal(e, $"Wystapil problem z otwieraniem procesu {processName}");
                await Task.Run(() => Notification.Danger.Send(e.Message + $" {processName}"));
            }
        }
    }
}