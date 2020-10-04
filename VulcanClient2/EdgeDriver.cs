using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace VulcanClient2
{
    public class EdgeDriver : IDriver
    {
        public string BrowserVersion { get; set; }
        public string Filename { get; set; }
        public string ZipFileName { get; set; }
        
        public EdgeDriver(string browserVersion)
        {
            BrowserVersion = browserVersion;
        }

        public async Task DownloadDriver()
        {
            string edgeVersion = BrowserVersion.Split(".")[0];
            Console.WriteLine("Sprawdzamy ostatnia wersje drivera do przegladrki");
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri($"https://msedgedriver.azureedge.net/LATEST_RELEASE_{edgeVersion}_WINDOWS");
                using (HttpResponseMessage lastestVersionResponse =
                    await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                using (Stream latestVersionStream = await lastestVersionResponse.Content.ReadAsStreamAsync())
                {
                    using (StreamReader sr = new StreamReader(latestVersionStream))
                    {
                        string latestDriverVersion = sr.ReadToEnd();
                        latestDriverVersion = String.Concat(latestDriverVersion.Where(c => !Char.IsWhiteSpace(c)));
                        Console.WriteLine($"Ostatnia wersja drivera {latestDriverVersion}");
                        Console.WriteLine("Pobieramy drivera");
                        Uri driverUri = new Uri($"https://msedgedriver.azureedge.net/{latestDriverVersion}/edgedriver_win64.zip");
                        using (HttpResponseMessage response = await client.GetAsync(driverUri, HttpCompletionOption.ResponseHeadersRead))
                        using (Stream driverStream = await response.Content.ReadAsStreamAsync())
                        {
                            string currentDirectory = Directory.GetCurrentDirectory();
                            string filePath = $"{currentDirectory}/edgedriver_win64.zip";
                            using (Stream driverStreamWrite = File.Open(filePath, FileMode.Create))
                            {
                                await driverStream.CopyToAsync(driverStreamWrite);
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
        }

        public IWebDriver GetDriver()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var options = new EdgeOptions();
            options.UseChromium = true;
            var service = EdgeDriverService.CreateChromiumService($"{currentDirectory}\\drivers\\");
            return new OpenQA.Selenium.Edge.EdgeDriver(service, options, TimeSpan.FromMinutes(3));
        }
    }
}