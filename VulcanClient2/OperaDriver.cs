using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Octokit;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using OpenQA.Selenium.Opera;


namespace VulcanClient2
{
    public class OperaDriver : IDriver
    {
        public string BrowserVersion { get; set; }
        public string Filename { get; set; }
        public string ZipFileName { get; set; }

        public OperaDriver(string browserVersion)
        {
            BrowserVersion = browserVersion;
        }
        
        public async Task DownloadDriver()
        {
            var client = new GitHubClient(new ProductHeaderValue("Vulcan-Client"));
            var releases = await client.Repository.Release.GetAll("operasoftware", "operachromiumdriver");
            string operaVersion = BrowserVersion.Split(".")[0];
            foreach (var release in releases)
            {
                Match match = Regex.Match(release.Body, @"Opera Stable "+ operaVersion);
                if (!match.Success) return;
                Console.WriteLine(release.Body);
                foreach (var asset in release.Assets)
                {
                    if (asset.Name == "operadriver_win64.zip")
                    {
                        string downloadUrl = asset.BrowserDownloadUrl;
                        using (HttpClient hClient = new HttpClient())
                        {
                            using (HttpResponseMessage response = await hClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                            {
                                string currentDirectory = Directory.GetCurrentDirectory();
                                string filePath = $"{currentDirectory}/{ZipFileName}";
                                using (Stream streamToWriteTo = File.Open(filePath, System.IO.FileMode.Create))
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
            }
        }

        public IWebDriver GetDriver()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var options = new OperaOptions();
            options.BinaryLocation = @"C:\Program Files\Opera\launcher.exe";
            var service = OperaDriverService.CreateDefaultService($"{currentDirectory}\\drivers\\operadriver_win64\\");
            return new OpenQA.Selenium.Opera.OperaDriver(service, options, TimeSpan.FromMinutes(3));
        }
    }
}