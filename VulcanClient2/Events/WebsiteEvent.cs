using System;
using System.IO;
using System.Threading.Tasks;
using SocketIOClient;
using Serilog;
using OpenQA.Selenium;

using VulcanClient2.Events;
using VulcanClient2.Webdriver;
using VulcanClient2.Webdriver.Actions;

namespace VulcanClient2.Events
{
    public class WebsiteEvent : Event
    {
        public static IWebDriver Driver { get; set; }
        public static Uri WebsiteUri { get; set; }
        
        public WebsiteEvent(SocketIO socket, string name, Notification notification) : base(socket, name, notification) {}
        
        static void RunWebDriver(DriverManager driverManager, Uri url, string actionTag=null)
        {

            Driver = driverManager.SelectedDriver.GetDriver();
            Driver.Manage().Window.Maximize();
            Progress.Set("website", 50);
            //Driver.Navigate().GoToUrl(url);
            try
            {
                HostManager hostManager = new HostManager(url, Driver);
                Log.Debug($"Host manager url: {hostManager.Url.ToString()}");
                var host = hostManager.GetHost();
                if (host != null && actionTag != null)
                {
                    IAction action = host.GetAction(actionTag);
                    action.DoAction();
                    Progress.Set("website", 100);
                }
                else
                {
                    Driver.Navigate().GoToUrl(url);
                    Progress.Set("website", 100);
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Wystapil problem z hostmenadzerem");
            }

        }
        
        public override async Task Run(SocketIOResponse response)
        {
            
            DriverManager driverManager = new DriverManager();
            var currentDirectory = Directory.GetCurrentDirectory();
            string url = response.GetValue(0).Value<string>("url");
            string actionTag = response.GetValue(0).Value<string>("action");
            bool urlIsOk = false;
            try
            {
                WebsiteUri = new Uri(url);
                urlIsOk = true;
                Progress.Set("website", 10);
            }
            catch (UriFormatException e)
            {
                await Task.Run(() => Notification.Danger.Send(e.Message));
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
                    await Task.Run(() => Notification.Danger.Send(e.Message));
                }

                if (driverPath != "")
                {
                    if (!File.Exists(driverPath))
                    {
                        Log.Debug("Driver nie istnieje");
                        await driverManager.SelectedDriver.DownloadDriver();

                        RunWebDriver(driverManager, WebsiteUri, actionTag);
                    }
                    else
                    {
                        Log.Debug("Driver istnieje");
                        RunWebDriver(driverManager, WebsiteUri, actionTag);
                    }

                    Notification.Success.Send($"Pomyslnie uruchomiono strone {url}");
                }
            }
        }
    }
}