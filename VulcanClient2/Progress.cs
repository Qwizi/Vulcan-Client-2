using SocketIOClient;
using System.Threading.Tasks;
using Serilog;

namespace VulcanClient2
{
    public class Progress
    {
        public static SocketIO Socket { get; set; }

        public static async Task Set(string progressSuffix, int value)
        {
            await Socket.EmitAsync("progress", new
            {
                idSuffix = progressSuffix,
                value = value
            });
        }
    }
}