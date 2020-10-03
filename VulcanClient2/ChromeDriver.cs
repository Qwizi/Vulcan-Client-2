using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace VulcanClient2
{
    public class ChromeDriver : IDriver
    {
        public string BrowserVersion { get; set; }
        public string Filename { get; set; }
        public string ZipFileName { get; set; }
        public ChromeDriver(string browserVersion)
        {
            BrowserVersion = browserVersion;
        }

        public async Task DownloadDriver()
        {
            string chromeVersion = BrowserVersion.Substring(0, 2);
            Console.WriteLine(chromeVersion);
            Console.WriteLine("Poberamy driver");
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
                            Console.WriteLine("Driver pobrany");
                        }

                        Console.WriteLine("Rozpakowujemy drivera");
                        await Task.Run(() =>
                        {
                            ZipFile.ExtractToDirectory(filePath, $"{currentDirectory}/drivers/");
                            Console.WriteLine("Driver rozpakowany");
                        });
                        Console.WriteLine("Usuwamy zipa");
                        await Task.Run(() =>
                        {
                            File.Delete(filePath);
                            Console.WriteLine("Zip usuniety");
                        });
                    }    
                }
            }
        }

        public IWebDriver GetDriver()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var options = new ChromeOptions();
            //options.AddArgument("no-sandbox");
            var service = ChromeDriverService.CreateDefaultService($"{currentDirectory}\\drivers\\");
            return new OpenQA.Selenium.Chrome.ChromeDriver(service, options, TimeSpan.FromMinutes(3));
            
        }
    }
}