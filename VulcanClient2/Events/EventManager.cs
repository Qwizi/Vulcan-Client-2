using System;
using SocketIOClient;
using System.Collections.Generic;

namespace VulcanClient2.Events
{
    public class EventManager
    {
        public List<IEvent> Events { get; set; }

        public EventManager()
        {
            Events = new List<IEvent>();
        }
        public  void Add(IEvent e)
        {
            Events.Add(e);
        }

        public List<IEvent> GetAll()
        {
            return Events;
        }
    }
}