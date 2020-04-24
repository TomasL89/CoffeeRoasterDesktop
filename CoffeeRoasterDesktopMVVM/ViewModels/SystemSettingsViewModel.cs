using CoffeeRoasterDesktopBackgroundLibrary;
using Prism.Commands;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class SystemSettingsViewModel : INotifyPropertyChanged, ITabViewModel, IDisposable
    {
        private readonly RoasterConnection roasterConnection;
        public int IpOne { get; set; } = 192;
        public int IpTwo { get; set; } = 168;
        public int IpThree { get; set; } = 1;
        public int IpFour { get; set; } = 2;
        public int PortNumber { get; set; } = 8180;
        public string Name { get; set; } = "System Settings";
        public string ConnectionStatus { get; set; } = "Disconnected";
        public ICommand OnWifiConnectPressed { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        private IDisposable connectionSubscription;

        public SystemSettingsViewModel(RoasterConnection connection)
        {
            if (connection.Configuration != null)
            {
                roasterConnection = connection;
                GetConfigurationData();
                PropertyChanged += SystemSettingsViewModel_PropertyChanged;
                OnWifiConnectPressed = new DelegateCommand(ConnectoToRoaster);
            }


            connectionSubscription = roasterConnection.WiFiConnected.Do(UpdateConncectionStatus).Subscribe();

        }

        private void UpdateConncectionStatus(bool connected)
        {
            ConnectionStatus = connected ? "Connected" : "Disconnected";
        }

        private void SystemSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ConnectoToRoaster();
        }

        private void ConnectoToRoaster()
        {
            var ipaddress = $"{IpOne}.{IpTwo}.{IpThree}.{IpFour}";

            var couldUpdateConfiguration = roasterConnection.UpdateConfiguration(ipaddress, PortNumber);

            if (couldUpdateConfiguration)
            {
                roasterConnection.ConnectToDevice();
            }
        }

        private void GetConfigurationData()
        {
            var ipaddress = roasterConnection.Configuration.IpAddress.Split(".");
            int.TryParse(ipaddress[0], out int ipOne);
            int.TryParse(ipaddress[1], out int ipTwo);
            int.TryParse(ipaddress[2], out int ipThree);
            int.TryParse(ipaddress[3], out int ipFour);

            IpOne = ipOne;
            IpTwo = ipTwo;
            IpThree = ipThree;
            IpFour = ipFour;

            PortNumber = roasterConnection.Configuration.PortNumber;
        }

        public void Dispose()
        {
            connectionSubscription.Dispose();
        }
    }
}
