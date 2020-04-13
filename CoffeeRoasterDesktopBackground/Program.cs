using Messages;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace CoffeeRoasterDesktopBackground
{
    // for testing only
    public class Program
    {
        private static WiFiManager wifiManager;
        static void Main(string[] args)
        {
           
            IDisposable disposable;
            // setup all processes
            var configurationService = new ConfigurationService();
            // todo testing only, shouldn't be able to set IpAddress like this
            configurationService.SystemConfiguration.IpAddress = "192.168.1.104";
            wifiManager = new WiFiManager(configurationService.SystemConfiguration);

            disposable = new CompositeDisposable
            {
                wifiManager.WiFiConnected.Subscribe(x => displayMessage(x)),
                MessageHandler.MessageRecieved.Subscribe(x => displayMessage(x))
            };

            var timer = new System.Timers.Timer(TimeSpan.FromSeconds(2).TotalSeconds);
            timer.AutoReset = true;
            timer.Elapsed += GetTemperatureMessage;
            timer.Start();


            Console.WriteLine("Running");
            while (true)
            {
                var inputMessage = Console.ReadLine();
                if (string.Equals(inputMessage, "quit", StringComparison.InvariantCultureIgnoreCase))
                    break;
                wifiManager.SendMessageToDevice("get");

            }
            Console.WriteLine("Closing Application");
            disposable.Dispose();
        }

        private static void GetTemperatureMessage(Object source, System.Timers.ElapsedEventArgs e)
        {
            wifiManager.SendMessageToDevice("get");
        }

        private static void displayMessage(object message)
        {
            if (message is TemperatureMessage)
            {
                var temperatureMessage = (TemperatureMessage)message;
                Console.WriteLine($"Temperature {temperatureMessage.Temperature}");
                Console.WriteLine($"Time {temperatureMessage.TimeInSeconds}");
            }
        }
    }
}
