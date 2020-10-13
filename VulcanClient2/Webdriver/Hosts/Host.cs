using System;
using System.Linq;
using System.Collections.Generic;
using OpenQA.Selenium;
using VulcanClient2.Webdriver.Actions;

namespace VulcanClient2.Webdriver.Hosts
{
    public interface IHost
    {
        public List<IAction> Actions { get; set; }
        public IWebDriver SeleniumDriver { get; }
        public Uri Url { get; }
        public IAction GetAction(string tag);
    }
    
    public class Host : IHost
    {
        public List<IAction> Actions { get; set; }
        public IWebDriver SeleniumDriver { get; }
        public Uri Url { get; }

        public Host(IWebDriver driver, Uri url)
        {
            SeleniumDriver = driver;
            Actions = new List<IAction>();
            Url = url;
        }

        public IAction GetAction(string tag)
        {
            return Actions.Find(a => a.Tag == tag);
        }
    }
}