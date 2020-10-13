using System;
using System.Threading.Tasks;
using SocketIOClient;

namespace VulcanClient2.Events
{
    public interface IEvent
    {
        public string Name { get; set; }
        public SocketIO Socket { get; }
        public Notification Notification { get; set; }
        
        public Task Run(SocketIOResponse response);
    }
    
    public abstract class Event : IEvent
    {
        public string Name { get; set; }
        public SocketIO Socket { get; set; }
        
        public Notification Notification { get; set; }

        public Event(SocketIO socket, string name, Notification notification=null)
        {
            Socket = socket;
            Name = name;
            Notification = notification;
        }
        
        public abstract Task Run(SocketIOResponse response);
    }
}