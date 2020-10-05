using System;
using System.Threading.Tasks;
using SocketIOClient;
namespace VulcanClient2
{
    public abstract class AbstractNotification
    {
        public SocketIO Socket { get; set; }
        public string Status { get; set; }

        public AbstractNotification(SocketIO socket)
        {
            Socket = socket;
        }
        
        public async void Send(string message)
        {
            await Socket.EmitAsync("notification", new
            {
                notification = new
                {
                    message = message,
                    pos = "bottom-right",
                    status = Status
                }
            });
        }
    }

    public class SuccessNotification : AbstractNotification
    {
        public SuccessNotification(SocketIO socket) : base(socket)
        {
            Status = "success";
        }
    }
    public class DangerNotification : AbstractNotification
    {
        public DangerNotification(SocketIO socket) : base(socket)
        {
            Status = "danger";
        }
    }

    public class Notification
    {
        public SuccessNotification Success { get; set; }
        public DangerNotification Danger { get; set; }
        
        public Notification(SocketIO socket)
        {
            Success = new SuccessNotification(socket);
            Danger = new DangerNotification(socket);
        }
    }
}