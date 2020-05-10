using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Input;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class SystemSettingsViewModel : INotifyPropertyChanged, ITabViewModel, IDisposable
    {
        public int IpOne { get; set; } = 192;
        public int IpTwo { get; set; } = 168;
        public int IpThree { get; set; } = 1;
        public int IpFour { get; set; } = 2;
        public int PortNumber { get; set; } = 8180;
        public string Name { get; set; } = "System Settings";
        public string ImageSource { get; } = "/Resources/interface (Freepik).png";
        public string ConnectionStatus { get; set; } = "Disconnected";

        public ICommand OnWifiConnectPressed { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConfigurationService configurationService;

        private readonly IDisposable roasterConnectionSubscription;

        public SystemSettingsViewModel(RoasterConnection roasterConnection)
        {
            roasterConnectionSubscription = roasterConnection.WifiConnectionChanged.ObserveOnDispatcher().Subscribe(UpdateConncectionStatus);
            configurationService = new ConfigurationService();
            GetConfigurationData();
            PropertyChanged += SystemSettingsViewModel_PropertyChanged;
            OnWifiConnectPressed = new DelegateCommand(ConnectoToRoaster);
            UpdateConncectionStatus(roasterConnection.Connected);
        }

        private void UpdateConncectionStatus(bool connected)
        {
            ConnectionStatus = connected ? "Connected" : "Disconnected";
        }

        private void SystemSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void ConnectoToRoaster()
        {
            var ipaddress = $"{IpOne}.{IpTwo}.{IpThree}.{IpFour}";

            var couldUpdateConfiguration = configurationService.UpdateConfiguration(ipaddress, PortNumber);
        }

        private void GetConfigurationData()
        {
            var ipaddress = configurationService.SystemConfiguration.IpAddress.Split(".");
            int.TryParse(ipaddress[0], out int ipOne);
            int.TryParse(ipaddress[1], out int ipTwo);
            int.TryParse(ipaddress[2], out int ipThree);
            int.TryParse(ipaddress[3], out int ipFour);

            IpOne = ipOne;
            IpTwo = ipTwo;
            IpThree = ipThree;
            IpFour = ipFour;

            PortNumber = configurationService.SystemConfiguration.PortNumber;
        }

        public void Dispose()
        {
            roasterConnectionSubscription.Dispose();
        }
    }
}