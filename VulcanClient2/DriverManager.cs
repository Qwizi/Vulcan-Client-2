using System;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Serilog;


namespace VulcanClient2
{
    public interface IDriver
    {
        string Filename { get; set; }
        string ZipFileName { get; set; }
        Task DownloadDriver();
        IWebDriver GetDriver();
    }

    public abstract class Driver : IDriver
    {
        public string BrowserVersion { get; set; }
        public string DriversPath { get; set; }
        public string ZipPath { get; set; }
        public string Filename { get; set; }
        public string ZipFileName { get; set; }
        public string CurrentDir { get; set; }

        public abstract Task DownloadZip();
        public abstract IWebDriver GetDriver();

        protected Driver(string browserVersion, string zipFileName, string filename)
        {
            BrowserVersion = browserVersion;
            ZipFileName = zipFileName;
            Filename = filename;
            CurrentDir = Directory.GetCurrentDirectory();
            ZipPath = $"{CurrentDir}/{ZipFileName}";
            DriversPath = $"{CurrentDir}/drivers/";
        }

        public void Unzip()
        {
            Log.Debug($"Rozpakowujemy drivera {ZipPath}");
            ZipFile.ExtractToDirectory(ZipPath, DriversPath);
            Log.Debug($"Driver rozpakowany {ZipPath}");
        }

        public void Delete()
        {
            Log.Debug($"Usuwamy zipa  {ZipPath}");
            File.Delete(ZipPath);
            Log.Debug($"Zip usuniety  {ZipPath}");
        }
        
        public async Task DownloadDriver()
        {
            Log.Debug("Pobieramy driver");
            await Task.Run(() => DownloadZip());
            Log.Debug("Driver pobrany");
            await Task.Run(() => Unzip());
            await Task.Run(() => Delete());
        }
    }

    public class DriverManager
    {
        public IDriver SelectedDriver;
        public DriverManager()
        {
            ApplicationFinder appFinder = new ApplicationFinder();
            string defaultBrowserName = appFinder.GetSystemDefaultBrowser();
            switch (defaultBrowserName)
            {
                case "Google Chrome":
                    string chromeVersion = appFinder.GetApplicationVersion("Google Chrome");
            
                    if (chromeVersion != "")
                    {
                        ChromeDriver chromeDriver = new ChromeDriver(chromeVersion, "chromedriver_win32.zip", "chromedriver.exe");
                        SelectedDriver = chromeDriver;
                    } 
                    break;
                case "Opera":
                    string operaVersion = appFinder.GetApplicationVersionRegex(@"^Opera Stable [0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$");

                    if (operaVersion != "")
                    {
                        OperaDriver operaDriver = new OperaDriver(operaVersion);
                        operaDriver.ZipFileName = "operadriver_win64.zip";
                        operaDriver.Filename = "operadriver_win64\\operadriver.exe";
                        SelectedDriver = operaDriver;
                    }

                    break;
                case "Microsoft Edge":
                    string edgeVersion = appFinder.GetApplicationVersion("Microsoft Edge");
                    if (edgeVersion != "")
                    {
                        EdgeDriver edgeDriver = new EdgeDriver(edgeVersion, "edgedriver_win64.zip", "msedgedriver.exe");
                        SelectedDriver = edgeDriver;
                    }
                    break;
            }
        }
    }
}