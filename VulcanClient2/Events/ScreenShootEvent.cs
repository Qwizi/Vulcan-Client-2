using System;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;
using System.Drawing.Imaging;

namespace VulcanClient2.Events
{
    public class ScreenShootEvent : Event
    {
        public ScreenShootEvent(SocketIO socket, string name) : base(socket, name) {}

        public override async Task Run(SocketIOResponse response)
        {
            ScreenShoot sc = new ScreenShoot();
            var imageBytes = sc.CaptureScreenToBytes(ImageFormat.Png);
            string filename = $"{Guid.NewGuid()}.png";
            Log.Information("Pomyslnie zrobiono zrzut ekranu");
            await Socket.EmitAsync("get_screenshoot", new
            {
                img = imageBytes,
                filename = filename
            });
        }
    }
}