using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace VulcanClient2.Webdriver.Drivers
{
    public class EdgeDriver : Driver
    {
       public EdgeDriver(string browserVersion, string zipFileName, string filename) : base (browserVersion, zipFileName, filename) {}

        public override async Task DownloadZip()
        {
            string edgeVersion = BrowserVersion.Split(".")[0];
            Console.WriteLine("Sprawdzamy ostatnia wersje drivera do przegladrki");
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri($"https://msedgedriver.azureedge.net/LATEST_RELEASE_{edgeVersion}_WINDOWS");
                using (HttpResponseMessage lastestVersionResponse =
                    await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    lastestVersionResponse.EnsureSuccessStatusCode();
                    using (Stream latestVersionStream = await lastestVersionResponse.Content.ReadAsStreamAsync())
                    {
                        using (StreamReader sr = new StreamReader(latestVersionStream))
                        {
                            string latestDriverVersion = sr.ReadToEnd();
                            latestDriverVersion = String.Concat(latestDriverVersion.Where(c => !Char.IsWhiteSpace(c)));
                            Console.WriteLine($"Ostatnia wersja drivera {latestDriverVersion}");
                            Console.WriteLine("Pobieramy drivera");
                            Uri driverUri =
                                new Uri($"https://msedgedriver.azureedge.net/{latestDriverVersion}/{ZipFileName}");
                            using (HttpResponseMessage response =
                                await client.GetAsync(driverUri, HttpCompletionOption.ResponseHeadersRead))
                            {
                                response.EnsureSuccessStatusCode();
                                using (Stream driverStream = await response.Content.ReadAsStreamAsync())
                                {
                                    using (Stream driverStreamWrite = File.Open(ZipPath, FileMode.Create))
                                    {
                                        await driverStream.CopyToAsync(driverStreamWrite);
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
        }

        public override IWebDriver GetDriver()
        {
            var options = new EdgeOptions();
            options.UseChromium = true;
            var service = EdgeDriverService.CreateChromiumService(DriversPath);
            return new OpenQA.Selenium.Edge.EdgeDriver(service, options, TimeSpan.FromMinutes(3));
        }
    }
}