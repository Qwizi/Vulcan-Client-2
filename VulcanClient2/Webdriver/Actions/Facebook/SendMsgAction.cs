using System;
using System.Threading.Tasks;
using Serilog;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace VulcanClient2.Webdriver.Actions.Facebook
{
    public class SendMsgAction : Action
    {
        public WebDriverWait Wait { get; set; }

        public SendMsgAction(string tag, IWebDriver driver, Uri url) : base(tag, driver, url)
        {
            Wait = new WebDriverWait(SeleniumDriver, TimeSpan.FromSeconds(30));
        }
        
        private bool IsElementPresent(By by)
        {
            try
            {
                SeleniumDriver.FindElement(by);
                return true;
            }
            catch (ElementNotInteractableException)
            {
                return false;
            }
        }

        public void Send(string msg)
        {
            // Piszemy i wysylamy
            Wait.Until(driver => driver.FindElement(By.XPath("//div[@data-block='true']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//div[@data-block='true']")).SendKeys("Hello World");
            SeleniumDriver.FindElement(By.XPath("//div[@data-block='true']")).SendKeys(Keys.Enter);
        }
        
        public override void DoAction()
        {
            SeleniumDriver.Manage().Window.Maximize();
            SeleniumDriver.Navigate().GoToUrl(Url);

            // Zamykanie modala z cookies
            Wait.Until(driver => driver.FindElement(By.XPath("//button[@data-testid='cookie-policy-banner-accept']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//button[@data-testid='cookie-policy-banner-accept']")).Click();
            
            // email input
            Wait.Until(driver => driver.FindElement(By.Id("email")).Displayed);
            SeleniumDriver.FindElement(By.Id("email")).SendKeys("3a2xDN@protonmail.com");
            
            // password input
            Wait.Until(driver => driver.FindElement(By.Id("pass")).Displayed);
            SeleniumDriver.FindElement(By.Id("pass")).SendKeys("8JEFRgUS7qHfiA2!49#");
            
            // login btn
            Wait.Until(driver => driver.FindElement(By.XPath("//button[@name='login']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//button[@name='login']")).Click();
            
            // Sprawdzamy czy strona sie zalogowala jezeli tak to przenonisimy na profil
            Wait.Until(driver => driver.FindElement(By.XPath("//div[@id='toolbarContainer']")));
            SeleniumDriver.Navigate().GoToUrl("https://www.facebook.com/profile.php?id=100006475886583");
            
            if (IsElementPresent(By.XPath("//button[text()='Dodaj']")))
            {
                SeleniumDriver.FindElement(By.XPath("//button[text()='Dodaj']")).Click();
            }
            // Klikamy w przycisk napisz wiadomosc
            Wait.Until(driver => driver.FindElement(By.XPath("//a[@data-loggable='ProfileHighQualityLogger']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//a[@data-loggable='ProfileHighQualityLogger']")).Click(); 
            
            // Klikamy w pole do pisania
            Wait.Until(driver => driver.FindElement(By.XPath("//div[@style='min-height: 16px;']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//div[@style='min-height: 16px;']")).Click();
            
            // Piszemy i wysylamy
            Send("Siiema chuju");
            
            SeleniumDriver.Close();
        }
    }
}