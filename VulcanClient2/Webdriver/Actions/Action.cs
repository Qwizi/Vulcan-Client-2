using System;
using OpenQA.Selenium;

namespace VulcanClient2.Webdriver.Actions
{
    public interface IAction
    {
        public string Tag { get; set; }
        public IWebDriver SeleniumDriver { get; set; }
        public Uri Url {get;}
        public void DoAction();
    }
    
    public abstract class Action : IAction
    {
        public string Tag { get; set; }
        public IWebDriver SeleniumDriver { get; set; }
        public Uri Url {get;}
        
        public Action(string tag, IWebDriver driver, Uri url)
        {
            Tag = tag;
            SeleniumDriver = driver;
            Url = url;
        }
        public abstract void DoAction();
    }
}