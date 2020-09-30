using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace VulcanClient2
{
    public interface IDriver
    {
        string Filename { get; set; }
        string ZipFileName { get; set; }
        void DownloadDriver(){}
        IWebDriver GetDriver();
    }

    public class ChromeDriver : IDriver
    {
        public string BrowserVersion { get; set; }
        public string Filename { get; set; }
        public string ZipFileName { get; set; }
        public ChromeDriver(string browserVersion)
        {
            BrowserVersion = browserVersion;
        }

        public async void DownloadDriver()
        {
            string chromeVersion = BrowserVersion.Substring(0, 2);
            Console.WriteLine(chromeVersion);
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage lastestVersionResponse =
                    await client.GetAsync($"http://chromedriver.storage.googleapis.com/LATEST_RELEASE_{chromeVersion}"))
                {
                    lastestVersionResponse.EnsureSuccessStatusCode();
                    string latestVersionResponseBody = await lastestVersionResponse.Content.ReadAsStringAsync();
                    string url = $"http://chromedriver.storage.googleapis.com/{latestVersionResponseBody}/{ZipFileName}";
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        string currentDirectory = Directory.GetCurrentDirectory();
                        string filePath = $"{currentDirectory}/{ZipFileName}";
                        using (Stream streamToWriteTo = File.Open(filePath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                        await Task.Run(() => ZipFile.ExtractToDirectory(filePath, $"{currentDirectory}/drivers/"));
                        await Task.Run(() => File.Delete(filePath));
                    }    
                }
            }
        }

        public IWebDriver GetDriver()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
            using (var service = ChromeDriverService.CreateDefaultService($"{currentDirectory}\\drivers\\"))
            {
                var driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options, TimeSpan.FromMinutes(3));
                return driver;
            }
        }
    }

    public class DriverManager
    {
        public IDriver SelectedDriver;
        public DriverManager()
        {
            List<IDriver> drivers = new List<IDriver>();
            
            ApplicationFinder appFinder = new ApplicationFinder();
            string chromeVersion = appFinder.getApplicationVersion("Google Chrome");
            if (chromeVersion != "")
            {
                ChromeDriver chromeDriver = new ChromeDriver(chromeVersion);
                chromeDriver.ZipFileName = "chromedriver_win32.zip";
                chromeDriver.Filename = "chromedriver.exe";
                drivers.Add(chromeDriver);
            }
            
            if (drivers.Count > 0) SelectedDriver = drivers[0];
            

        }
    }
}