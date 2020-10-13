using System;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;

namespace VulcanClient2.Events
{
    public class WallPaperEvent : Event
    {
        public WallPaperEvent(SocketIO socket, string name) : base(socket, name) {}
        
        public override async Task Run(SocketIOResponse response)
        {
            string wallperUrl = response.GetValue(0).Value<string>("wallper_url");
            Log.Debug(wallperUrl);
            Uri wallperUri = new Uri(wallperUrl);
            Log.Debug(wallperUri.ToString());
            Wallper.Set(wallperUri, Wallper.Style.Stretched);
        }
    }
}