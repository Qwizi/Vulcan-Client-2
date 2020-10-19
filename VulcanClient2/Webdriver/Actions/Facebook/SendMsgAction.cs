using System;
using Serilog;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
            SeleniumDriver.FindElement(By.XPath("//div[@data-block='true']")).SendKeys(msg);
            SeleniumDriver.FindElement(By.XPath("//div[@data-block='true']")).SendKeys(Keys.Enter);
        }
        
        public override async Task DoAction()
        {
            string address = Config.Instance.GetSection("Adress").Value + "api/accounts";
            Log.Debug(address);
            var addressUri = new Uri(address);
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(addressUri))
                {
                    response.EnsureSuccessStatusCode();
                    JToken jsonResponse = JToken.Parse(await response.Content.ReadAsStringAsync());
                    ;
                    if (jsonResponse.Value<int>("count") > 0)
                    {
                        
                    }
                    Log.Debug(jsonResponse.ToString());
                }
            }
            
            SeleniumDriver.Navigate().GoToUrl("https://www.facebook.com");
            Progress.Set("website", 60);

            // Zamykanie modala z cookies
            Wait.Until(driver => driver.FindElement(By.XPath("//button[@data-testid='cookie-policy-banner-accept']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//button[@data-testid='cookie-policy-banner-accept']")).Click();
            Progress.Set("website", 65);
            
            // email input
            Wait.Until(driver => driver.FindElement(By.Id("email")).Displayed);
            SeleniumDriver.FindElement(By.Id("email")).SendKeys("3a2xDN@protonmail.com");

            // password input
            Wait.Until(driver => driver.FindElement(By.Id("pass")).Displayed);
            SeleniumDriver.FindElement(By.Id("pass")).SendKeys("8JEFRgUS7qHfiA2!49#");

            // login btn
            Wait.Until(driver => driver.FindElement(By.XPath("//button[@name='login']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//button[@name='login']")).Click();
            Progress.Set("website", 70);
            
            // Sprawdzamy czy strona sie zalogowala jezeli tak to przenonisimy na profil
            Wait.Until(driver => driver.FindElement(By.XPath("//div[@id='toolbarContainer']")));
            SeleniumDriver.Navigate().GoToUrl(Url);
            Progress.Set("website", 75);

            // Klikamy w przycisk napisz wiadomosc
            Wait.Until(driver => driver.FindElement(By.XPath("//a[@data-loggable='ProfileHighQualityLogger']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//a[@data-loggable='ProfileHighQualityLogger']")).Click(); 
            Progress.Set("website", 80);
            
            // Klikamy w pole do pisania
            Wait.Until(driver => driver.FindElement(By.XPath("//div[@style='min-height: 16px;']")).Displayed);
            SeleniumDriver.FindElement(By.XPath("//div[@style='min-height: 16px;']")).Click();
            Progress.Set("website", 85);

            string[] messages = {"Siema", "Co robisz?", "Chuju odpisz", "jan pawel 2 gwalcil male dzieci", "Jebać cie", "chcesz cos z ivonu?"};
            var random = new Random();
            // Piszemy i wysylamy
            for (int i = 0; i < 10; i++)
            {
                int msgIndex = random.Next(messages.Length);
                Send(messages[msgIndex]);
                Thread.Sleep(2000);
            }
                        
            Progress.Set("website", 90);
            Thread.Sleep(5000);
            SeleniumDriver.Close();
            Progress.Set("website", 95);
        }
    }
}