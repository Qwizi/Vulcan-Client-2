using System;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenQA.Selenium;


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
            Console.WriteLine("Rozpakowujemy drivera");
            ZipFile.ExtractToDirectory(ZipPath, DriversPath);
            Console.WriteLine("Driver rozpakowany");
        }

        public void Delete()
        {
            Console.WriteLine("Usuwamy zipa");
            File.Delete(ZipPath);
            Console.WriteLine("Zip usuniety");
        }
        
        public async Task DownloadDriver()
        {
            Console.WriteLine("Pobieramy driver");
            await Task.Run(() => DownloadZip());
            Console.WriteLine("Driver pobrany");
            await Task.Run(() => Unzip());
            await Task.Run(() => Delete());
        }
    }

    public class DriverManager
    {
        public IDriver SelectedDriver;
        public DriverManager()
        {
            List<IDriver> drivers = new List<IDriver>();
            
            ApplicationFinder appFinder = new ApplicationFinder();
            string chromeVersion = appFinder.GetApplicationVersion("Google Chrome");
            
            if (chromeVersion != "")
            {
                ChromeDriver chromeDriver = new ChromeDriver(chromeVersion, "chromedriver_win32.zip", "chromedriver.exe");
                drivers.Add(chromeDriver);
            }
            
            string operaVersion = appFinder.GetApplicationVersionRegex(@"^Opera Stable [0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$");

            if (operaVersion != "")
            {
                OperaDriver operaDriver = new OperaDriver(operaVersion);
                operaDriver.ZipFileName = "operadriver_win64.zip";
                operaDriver.Filename = "operadriver_win64\\operadriver.exe";
                drivers.Add(operaDriver);
            }

            string edgeVersion = appFinder.GetApplicationVersion("Microsoft Edge");
            if (edgeVersion != "")
            {
                EdgeDriver edgeDriver = new EdgeDriver(edgeVersion, "edgedriver_win64.zip", "msedgedriver.exe");
                drivers.Add(edgeDriver);
            }

            if (drivers.Count > 0) SelectedDriver = drivers[2];
        }
    }
}