using System;
using OpenQA.Selenium;
using System.Collections.Generic;
using Serilog;
using VulcanClient2.Webdriver.Actions.Facebook;

namespace VulcanClient2.Webdriver.Hosts
{
    public class FacebookHost : Host
    {
        public FacebookHost(IWebDriver driver, Uri url) : base(driver, url)
        {
            Log.Debug("FacebookHost zaladowany");
            Actions.Add(new SendMsgAction("send_msg", SeleniumDriver, url));
        }
    }
}