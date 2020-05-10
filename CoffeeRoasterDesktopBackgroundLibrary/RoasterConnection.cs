using CoffeeRoasterDesktopBackground;
using Messages;
using SimpleTCP;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class RoasterConnection
    {
        private RoasterConnection()
        {
            configurationService = new ConfigurationService();
            Configuration = configurationService.SystemConfiguration;
            //client = new SimpleTcpClient();
            client = new SimpleTcpClient();

            client.DataReceived += Client_DataReceived;
            client.DelimiterDataReceived += Client_DelimiterDataReceived;

            //var task = new Task(() =>
            //{
            //    //  client.DataReceived += Client_DataReceived;
            //    for (; ; )
            //    { }
            //});
            //task.Start();
            Connect();
        }

        private static RoasterConnection instance;

        private static readonly object _lock = new object();

        public static RoasterConnection GetConnectionInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new RoasterConnection();
                    }
                }
            }

            return instance;
        }

        public IObservable<bool> WifiConnectionChanged => wiFiConnectedSubject;
        public IObservable<IMessage> MessageRecieved => messageRecievedSubject;

        private readonly Subject<IMessage> messageRecievedSubject = new Subject<IMessage>();
        private readonly Subject<bool> wiFiConnectedSubject = new Subject<bool>();

        private SimpleTcpClient client;
        public Configuration Configuration { get; set; }
        public bool Connected { get; private set; }
        private readonly ConfigurationService configurationService;
        private string lastMessageReceived;

        private void Client_DelimiterDataReceived(object sender, Message e)
        {
        }

        public bool UpdateConfiguration(string ipAddress, int portNumber)
        {
            Configuration.IpAddress = ipAddress;
            Configuration.PortNumber = portNumber;

            return configurationService.SaveConfiguration(Configuration);
        }

        public void ConnectToDevice()
        {
            Connect();
        }

        private void Connect()
        {
            Connected = false;
            try
            {
                client.Connect(Configuration.IpAddress, Configuration.PortNumber);
                wiFiConnectedSubject.OnNext(client.TcpClient.Connected);
                Connected = true;
            }
            catch (Exception)
            {
                wiFiConnectedSubject.OnNext(false);
            }
        }

        public void StartRoast()
        {
            SendMessageToDevice("Start");
        }

        private void Client_DataReceived(object sender, Message e)
        {
            HandleIncomingMessage(e.MessageString);
        }

        public void SendMessageToDevice(string message)
        {
            if (client.TcpClient.Connected)
                client.WriteLine(message);

            // if (client.TcpClient != null)
        }

        public string SendMessageToDeviceWithReply(string message)
        {
            var result = client?.WriteLineAndGetReply(message, TimeSpan.FromSeconds(5))?.MessageString;
            return result;
        }

        public void HandleIncomingMessage(string message)
        {
            if (!string.Equals(lastMessageReceived, message, StringComparison.InvariantCultureIgnoreCase))
            {
                var incomingMessage = message.ToString().Split(":");

                if (incomingMessage.Count() == 0)
                    return;

                int.TryParse(incomingMessage[0], out int messageType);

                switch (messageType)
                {
                    case 1:
                        HandleTemperatureMessage(incomingMessage);
                        break;

                    case 2:
                        HandleSystemMessage(incomingMessage);
                        break;

                    case 3:
                        HandleErrorMessage(incomingMessage);
                        break;

                    case 0:
                    default:
                        break;
                }
                lastMessageReceived = message;
            }
        }

        private void HandleErrorMessage(string[] messageData)
        {
        }

        private void HandleSystemMessage(string[] messageData)
        {
        }

        private void HandleTemperatureMessage(string[] messageData)
        {
            int.TryParse(messageData[1], out int temperature);
            int.TryParse(messageData[2], out int timeInSeconds);
            bool.TryParse(messageData[3], out bool heaterOn);
            float.TryParse(messageData[4], out float percentageComplete);

            messageRecievedSubject.OnNext(new TemperatureMessage()
            {
                Temperature = temperature,
                TimeInSeconds = timeInSeconds,
                HeaterOn = heaterOn,
                RoastProgress = percentageComplete
            });
        }
    }
}