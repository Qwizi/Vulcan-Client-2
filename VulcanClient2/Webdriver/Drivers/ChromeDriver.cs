using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace VulcanClient2.Webdriver.Drivers
{
    public class ChromeDriver : Driver
    {
        
        public ChromeDriver(string browserVersion, string zipFileName, string filename) : base(browserVersion, zipFileName, filename){}

        public override async Task DownloadZip()
        {
            string chromeVersion = BrowserVersion.Substring(0, 2);
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
                        using (Stream streamToWriteTo = File.Open(ZipPath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }    
                }
            }
        }

        public override IWebDriver GetDriver()
        {
            var options = new ChromeOptions();
            var service = ChromeDriverService.CreateDefaultService(DriversPath);
            return new OpenQA.Selenium.Chrome.ChromeDriver(service, options, TimeSpan.FromMinutes(3));
            
        }
    }
}