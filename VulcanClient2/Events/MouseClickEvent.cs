using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;

namespace VulcanClient2.Events
{
    public class MouseClickEvent : Event
    {
        public MouseClickEvent(SocketIO socket, string name) : base(socket, name) {}

        public override async Task Run(SocketIOResponse response)
        {
            string type = response.GetValue(0).Value<string>("type");
            switch (type)
            {
                case "left":
                    Mouse.Click(Win32.MouseEventFlags.LeftDown | Win32.MouseEventFlags.LeftUp);
                    break; 
                case "right":
                    Mouse.Click(Win32.MouseEventFlags.RightDown | Win32.MouseEventFlags.RightUp);
                    break;
                case "middle":
                    Mouse.Click(Win32.MouseEventFlags.MiddleDown | Win32.MouseEventFlags.MiddleUp);
                    break;
            }
        }
    }
}