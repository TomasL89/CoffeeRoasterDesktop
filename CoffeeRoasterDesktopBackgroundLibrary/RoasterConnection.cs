using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary.RoastProfile;
using Messages;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class RoasterConnection : IDisposable
    {
        private readonly ConfigurationService configurationService;
        private string lastMessageReceived;
        private SimpleTcpClient client;

        private readonly Subject<IMessage> messageRecievedSubject = new Subject<IMessage>();
        private readonly Subject<bool> wiFiConnectedSubject = new Subject<bool>();
        private readonly Subject<RoastProfile.RoastProfile> profileRecievedSubject = new Subject<RoastProfile.RoastProfile>();
        private readonly Subject<string> wifiStrengthUpdateSubject = new Subject<string>();

        private static RoasterConnection instance;
        private static readonly object _lock = new object();

        public IObservable<bool> WiFiConnectionChanged => wiFiConnectedSubject;
        public IObservable<IMessage> MessageRecieved => messageRecievedSubject;
        public IObservable<RoastProfile.RoastProfile> ProfileRecieved => profileRecievedSubject;
        public IObservable<string> WiFiStrengthUpdated => wifiStrengthUpdateSubject;
        public Configuration Configuration { get; set; }
        private string profileShell = string.Empty;
        private List<string> profilePoints = new List<string>();
        private readonly ProfileService profileService;
        private IDisposable heartbeatSubscription;
        private DateTime lastMessageTimeStamp;

        private RoasterConnection()
        {
            profileService = new ProfileService();
            configurationService = new ConfigurationService();
            Configuration = configurationService.SystemConfiguration;
            client = new SimpleTcpClient();
            client.DataReceived += Client_DataReceived;
            client.DelimiterDataReceived += Client_DelimiterDataReceived;
            heartbeatSubscription = Observable.Interval(TimeSpan.FromSeconds(10)).Do(SendHeartBeat).Subscribe();
            Connect();
        }

        private void SendHeartBeat(long obj)
        {
            var currentTime = DateTime.Now;

            if (currentTime - lastMessageTimeStamp > TimeSpan.FromSeconds(10))
                SendMessageToDevice("WiFi Get");
        }

        private void Client_DelimiterDataReceived(object sender, Message e)
        {
            Debug.WriteLine(e);
        }

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

        public void StartRoast()
        {
            if (client.TcpClient.Connected)
            {
                SendMessageToDevice("Start");
            }
            else
            {
                wiFiConnectedSubject.OnNext(false);
            }
        }

        private void Client_DataReceived(object sender, Message e)
        {
            HandleIncomingMessage(e.MessageString);
        }

        public void SendMessageToDevice(string message)
        {
            try
            {
                if (client.TcpClient.Connected)
                {
                    client.WriteLine(message);
                }
                else
                {
                    wiFiConnectedSubject.OnNext(client.TcpClient.Connected);
                }
            }
            catch (Exception)
            {
                wiFiConnectedSubject.OnNext(false);
            }
        }

        public void SendProfileToRoaster(string profileMessage, int attempts)
        {
            if (string.IsNullOrWhiteSpace(profileMessage))
                return;

            if (!client.TcpClient.Connected)
            {
                wiFiConnectedSubject.OnNext(client.TcpClient.Connected);
                return;
            }

            try
            {
                var task = new Task(() =>
                {
                    // todo this crashes everything
                    var strippedProfileMessage = profileMessage.Replace("\r", "").Replace("\n", "");
                    for (var i = 0; i < attempts; i++)
                    {
                        var result = client?.WriteLineAndGetReply("Profile Set" + strippedProfileMessage + '\n', TimeSpan.FromSeconds(5))?.MessageString;
                        if (result == null)
                            continue;

                        if (string.Equals(result, "valid_profile", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        Thread.Sleep(2000);
                    }
                });
                task.Start();
            }
            catch (Exception)
            {
            }
        }

        public void RequestProfileFromDevice()
        {
            try
            {
                if (client.TcpClient.Connected)
                {
                    client?.WriteLine("Profile Get");
                    return;
                }
            }
            catch (Exception)
            {
                wiFiConnectedSubject.OnNext(false);
            }
        }

        public string SendMessageToDeviceWithReply(string message)
        {
            try
            {
                if (!client.TcpClient.Connected)
                {
                    wiFiConnectedSubject.OnNext(client.TcpClient.Connected);
                    return null;
                }

                var result = client?.WriteLineAndGetReply(message, TimeSpan.FromSeconds(10))?.MessageString;
                return result;
            }
            catch (Exception)
            {
                wiFiConnectedSubject.OnNext(false);
                return null;
            }
        }

        public void HandleIncomingMessage(string message)
        {
            if (!string.Equals(lastMessageReceived, message, StringComparison.InvariantCultureIgnoreCase))
            {
                lastMessageTimeStamp = DateTime.Now;

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
                        HandleWiFiStatusMessage(incomingMessage);
                        break;

                    case 3:
                        HandleErrorMessage(incomingMessage);
                        break;

                    case 4:
                        HandleProfileMessage(message);
                        break;

                    case 0:
                    default:
                        break;
                }
                lastMessageReceived = message;
            }
        }

        private void HandleProfileMessage(string message)
        {
            var profileSection = message.Replace("4:", "");
            // Build the profile shell first
            if (profileSection.Contains("RoastName"))
            {
                profileShell = profileSection;
            }
            // Add each profile point to a list
            if (profileSection.Contains("StagePoint"))
            {
                profileSection.Replace(Environment.NewLine, string.Empty);
                // todo this library is not working as expected, the points are arriving as an entire block
                var points = profileSection.Split("\n");
                if (points.Count() == 1)
                {
                    profilePoints.Add(profileSection);
                }
                else if (points.Count() > 1)
                {
                    foreach (var point in points)
                    {
                        if (point.Length < 10)
                            continue;
                        if (point.Contains("end transmission"))
                            continue;
                        profilePoints.Add(point);
                    }
                }
            }
            // When the profile has finished sending rebuild the profile
            if (profileSection.Contains("end transmission"))
            {
                var newProfile = profileService.RebuildProfile(profileShell, profilePoints);
                if (newProfile != null)
                {
                    profileRecievedSubject.OnNext(newProfile);
                }
                profileShell = string.Empty;
                profilePoints.Clear();
            }
        }

        private void Connect()
        {
            try
            {
                Configuration = new ConfigurationService().LoadConfiguration();
                client.Connect(Configuration.IpAddress, Configuration.PortNumber);
                wiFiConnectedSubject.OnNext(client.TcpClient.Connected);
            }
            catch (Exception)
            {
                wiFiConnectedSubject.OnNext(false);
            }
        }

        private void HandleErrorMessage(string[] messageData)
        {
        }

        private void HandleWiFiStatusMessage(string[] messageData)
        {
            int.TryParse(messageData[1], out int rssi);
            if (rssi == 0)
                return;
            else if (rssi > -67)
                wifiStrengthUpdateSubject.OnNext("Excellent");
            else if (rssi > -70 && rssi < -67)
                wifiStrengthUpdateSubject.OnNext("Very Good");
            else if (rssi >= -80 && rssi < -70)
                wifiStrengthUpdateSubject.OnNext("Poor");
            else if (rssi < -90)
                wifiStrengthUpdateSubject.OnNext("Unusable");
        }

        private void HandleTemperatureMessage(string[] messageData)
        {
            int.TryParse(messageData[1], out int temperature);
            int.TryParse(messageData[2], out int timeInSeconds);
            bool heaterOn = messageData[3] == "1" ? true : false;
            float.TryParse(messageData[4], out float percentageComplete);

            messageRecievedSubject.OnNext(new TemperatureMessage()
            {
                Temperature = temperature,
                TimeInSeconds = timeInSeconds,
                HeaterOn = heaterOn,
                RoastProgress = percentageComplete
            });
        }

        public void Dispose()
        {
            heartbeatSubscription?.Dispose();
        }
    }
}