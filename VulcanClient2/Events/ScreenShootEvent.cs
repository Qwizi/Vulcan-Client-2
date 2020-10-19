using System;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;
using System.Drawing.Imaging;

namespace VulcanClient2.Events
{
    public class ScreenShootEvent : Event
    {
        public ScreenShootEvent(SocketIO socket, string name, Notification notification) : base(socket, name, notification) {}

        public override async Task Run(SocketIOResponse response)
        {
            ScreenShoot sc = new ScreenShoot();
            Progress.Set("screenshoot", 30);
            var imageBytes = sc.CaptureScreenToBytes(ImageFormat.Png);
            Progress.Set("screenshoot", 70);
            string filename = $"{Guid.NewGuid()}.png";
            Log.Information("Pomyslnie zrobiono zrzut ekranu");
            await Socket.EmitAsync("get_screenshoot", new
            {
                img = imageBytes,
                filename = filename,
                clientId = $"/clients#{Socket.Id}"
            });
            Progress.Set("screenshoot", 100);
            Notification.Success.Send("Pomyslnie zrobiono zrzut ekranu");
        }
    }
}