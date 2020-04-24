using CoffeeRoasterDesktopBackground;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using SimpleTCP;

namespace CoffeeRoasterDesktopBackgroundLibrary
{
    public class RoasterConnection
    {
        public IObservable<bool> WiFiConnected => wiFiConnectedSubject;
        public IObservable<IMessage> MessageRecieved => messageRecievedSubject;

        private readonly Subject<IMessage> messageRecievedSubject = new Subject<IMessage>();
        private readonly Subject<bool> wiFiConnectedSubject = new Subject<bool>();

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        SimpleTcpClient client;
        private int portNumber;
        private string ipAddress;

        // todo make a private field 
        public Configuration Configuration { get; set; }

        private readonly ConfigurationService configurationService;
        public RoasterConnection()
        {
            configurationService = new ConfigurationService();
            Configuration = configurationService.SystemConfiguration;
            ipAddress = Configuration.IpAddress;
            portNumber = Configuration.PortNumber;


        }

        public bool UpdateConfiguration(string ipAddress, int portNumber)
        {
            Configuration.IpAddress = ipAddress;
            Configuration.PortNumber = portNumber;

            return configurationService.SaveConfiguration(Configuration);
        }

        public void ConnectToDevice()
        {

            var task = new Task(() => { Connect(); });
            task.Start();
        }

        private void Connect()
        {
            try
            {
                client = new SimpleTcpClient().Connect(Configuration.IpAddress, Configuration.PortNumber);
                client.DataReceived += Client_DataReceived;
                wiFiConnectedSubject.OnNext(client.TcpClient.Connected);
            }
            catch (Exception)
            {
                wiFiConnectedSubject.OnNext(false);
            }
            
           
        }

        private void Client_DataReceived(object sender, Message e)
        {
            HandleIncomingMessage(e.MessageString);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Client_Connected(object sender, EventArgs e)
        {

        }

        public void SendMessageToDevice(string message)
        {
            // if (client.TcpClient != null)
            //client.Send(message);
        }

        public void HandleIncomingMessage(string message)
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
                    Console.WriteLine(message);
                    break;
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

            messageRecievedSubject.OnNext(new TemperatureMessage()
            {
                Temperature = temperature,
                TimeInSeconds = timeInSeconds
            });
        }
    }
}
