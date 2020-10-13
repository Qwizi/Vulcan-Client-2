using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;

namespace VulcanClient2.Events
{
    public class MouseEvent : Event
    {
        public MouseEvent (SocketIO socket, string name) : base(socket, name) {}

        public override async Task Run(SocketIOResponse response)
        {
            Log.Debug(response.ToString());
            string type = response.GetValue(0).Value<string>("type");
            Log.Debug(type);
            int p = response.GetValue(0).Value<int>("p");
            switch(type)
            {
                case "x":
                    Mouse.MoveX(p);
                    Log.Debug($"Przesumany x o {p.ToString()}");
                    break;
                            
                case "y":
                    Mouse.MoveY(p);
                    Log.Debug($"Przesumany y o {p.ToString()}");
                    break;
            }
        }
    }
}