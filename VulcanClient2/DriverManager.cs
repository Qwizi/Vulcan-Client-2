using System;
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
                ChromeDriver chromeDriver = new ChromeDriver(chromeVersion);
                chromeDriver.ZipFileName = "chromedriver_win32.zip";
                chromeDriver.Filename = "chromedriver.exe";
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

            if (drivers.Count > 0) SelectedDriver = drivers[1];
        }
    }
}