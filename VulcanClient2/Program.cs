using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SocketIOClient;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using  System.Media;
namespace VulcanClient2
{
    class ProcessDict
    {
        public string Name { get; set; }
        public int Id { get; set; }
        
        public string MainWindowTitle { get; set; }
    }
    
    class Program
    {
        public static IWebDriver WebDriver {get; set;}
        static void RunWebDriver(DriverManager driverManager, Uri url)
        {
            try
            {
                WebDriver = driverManager.SelectedDriver.GetDriver();
                WebDriver.Navigate().GoToUrl(url);
            }
            catch (WebDriverException e)
            {
                Console.WriteLine(e);
            }
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("Vulcan Client v.1.0.2");
            Console.OutputEncoding = Encoding.UTF8;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");
            
            var config = builder.Build();
            var adress = config.GetSection("Adress").Value;

            var uri = new Uri(adress+"clients");
            var socket = new SocketIO(uri);
            var notification = new Notification(socket);
            

            socket.OnConnected += Socket_OnConnected;
            socket.OnDisconnected += Socket_onDisconnected;

            socket.On("website", async response =>
            {
                DriverManager driverManager = new DriverManager();
                var currentDirectory = Directory.GetCurrentDirectory();
                string url = response.GetValue(0).Value<string>("url");
                bool urlIsOk = false; ;
                try
                {
                    Uri uri = new Uri(url);
                    urlIsOk = true;
                }
                catch (UriFormatException e)
                {
                    await Task.Run(() => notification.Danger.Send(e.Message));
                }

                if (urlIsOk)
                {
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
                }
            });
            
            socket.On("command", async response =>
            {
                string cmd = "/C " + response.GetValue<string>();

                var process = new Process();
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = cmd;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                
                process.Start();

                await process.WaitForExitAsync();

                StreamReader reader = process.StandardOutput;
                StreamReader errorReader = process.StandardError;
                
                string cmdOutput = reader.ReadToEnd();
                string cmdError = errorReader.ReadToEnd();
                Console.WriteLine(process.StandardOutput.ReadToEnd());

                string output = cmdError != "" ? cmdError : cmdOutput;

                Console.WriteLine(output);
                Console.WriteLine(process.ExitCode);
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
            
            socket.On("process_list", async response =>
            {
                List<ProcessDict> processList = new List<ProcessDict>();

                foreach (var process in Process.GetProcesses().Where(p => p.MainWindowTitle != "").ToArray())
                {
                    processList.Add(new ProcessDict {Name = process.ProcessName, Id = process.Id, MainWindowTitle = process.MainWindowTitle});
                }
                
                await socket.EmitAsync("process_list", new
                {
                    processes = processList
                });
            });
            
            socket.On("process_kill", async response =>
            {
                int processId = response.GetValue(0).Value<int>("processId");
                foreach (var process in Process.GetProcesses().Where(p => p.MainWindowTitle != "").ToArray())
                {
                    if (process.Id == processId)
                    {
                        process.Kill();
                        await Task.Run(() => notification.Success.Send($"Pomyslnie zamknieto proces {processId}"));
                    }
                }
            });
            
            socket.On("process_start", async response =>
            {
                string processName = response.GetValue(0).Value<string>("processName");
                var process = new Process();
                process.StartInfo.FileName = processName;
                try
                {
                    process.Start();
                    await Task.Run(() => notification.Success.Send($"Pomyslnie uruchomino {processName}"));
                }
                catch (Win32Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.Run(() => notification.Danger.Send(e.Message + $" {processName}"));
                }
                
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