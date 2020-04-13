using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using SimpleTCP;
using System.Threading.Tasks;

namespace CoffeeRoasterDesktopBackground
{
    public class WiFiManager
    {
        public IObservable<bool> WiFiConnected => wiFiConnectedSubject;

        SimpleTcpClient client;
        private Subject<bool> wiFiConnectedSubject = new Subject<bool>();
        private int portNumber;
        private string ipAddress;
        public WiFiManager(Configuration configuration)
        {
            ipAddress = configuration.IpAddress;
            portNumber = configuration.PortNumber;

            client = new SimpleTcpClient();
            client.Connect(ipAddress, portNumber);
            client.DataReceived += TcpServer_DataReceived;
            // todo fix this
            if (client.TcpClient.Connected)
            {
                wiFiConnectedSubject.OnNext(true);
            }
        }

        private void TcpServer_DataReceived(object sender, Message e)
        {
            MessageHandler.HandleIncomingMessage(e.MessageString);
        }

        public void SendMessageToDevice(string message)
        {
            client.Write(message);
        }

    }
}
