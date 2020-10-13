using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;

namespace VulcanClient2.Events
{
    public class ProcessDict
    {
        public string Name { get; set; }
        public int Id { get; set; }
        
        public string MainWindowTitle { get; set; }
    }
    
    public class ProcessListEvent : Event
    {
        public ProcessListEvent(SocketIO socket, string name) : base(socket, name) {}

        public override async Task Run(SocketIOResponse response)
        {
            List<ProcessDict> processList = new List<ProcessDict>();

            foreach (var process in Process.GetProcesses().Where(p => p.MainWindowTitle != "").ToArray())
            {
                processList.Add(new ProcessDict
                {
                    Name = process.ProcessName, 
                    Id = process.Id,
                    MainWindowTitle = process.MainWindowTitle
                });
            }

            await Socket.EmitAsync("process_list", new
            {
                processes = processList
            });
        }
    }
}