using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary;
using Messages;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CoffeeRoasterDesktopCLI
{
    // for testing only
    public class Program
    {
        private static RoasterConnection roasterConnection;
        static async Task Main(string[] args)
        {
           
            IDisposable disposable;
            // setup all processes
            var configurationService = new ConfigurationService();
            // todo testing only, shouldn't be able to set IpAddress like this
            roasterConnection = new RoasterConnection();
            //await roasterConnection.UpdateConfigurationAsync("192.168.1.109", 8180);

            roasterConnection.UpdateConfiguration("192.168.1.109", 8180);
            Console.WriteLine($"Attempting connection on 192.168.1.109: {8180}");
            roasterConnection.ConnectToDevice();
            disposable = new CompositeDisposable
            {
                roasterConnection.WiFiConnected.Subscribe(x => displayMessage(x)),
                roasterConnection.MessageRecieved.Subscribe(x => displayMessage(x))
            };
            Console.WriteLine("Connected");
            //var timer = new System.Timers.Timer();
            //timer.AutoReset = true;
            //timer.Interval = 5000;
            //timer.Elapsed += GetTemperatureMessage;
            //timer.Enabled = true;

            Console.WriteLine("Running");
            while (true)
            {
                var inputMessage = Console.ReadLine();
                if (string.Equals(inputMessage, "quit", StringComparison.InvariantCultureIgnoreCase))
                    break;
                roasterConnection.SendMessageToDevice("get");

            }
            Console.WriteLine("Closing Application");
            disposable.Dispose();
        }

        private static void GetTemperatureMessage(Object source, System.Timers.ElapsedEventArgs e)
        {
            roasterConnection.SendMessageToDevice("get");
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
