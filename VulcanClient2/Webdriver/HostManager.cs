using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Serilog;
using VulcanClient2.Webdriver.Hosts;

namespace VulcanClient2.Webdriver
{
    public class HostManager
    {
        public Uri Url { get; }
        private IWebDriver SeleniumDriver { get; }
        
        public HostManager(Uri url, IWebDriver seleniumDriver)
        {
            SeleniumDriver = seleniumDriver;
            Url = url;
        }

        public IHost GetHost()
        {
            string host = Url.Host;
            if (host == "www.facebook.com" || host == "facebook.com")
            {
                return new FacebookHost(SeleniumDriver, Url);
            }
            return null;
        }
    }
}