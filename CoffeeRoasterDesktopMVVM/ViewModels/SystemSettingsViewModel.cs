using CoffeeRoasterDesktopBackground;
using CoffeeRoasterDesktopBackgroundLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Documents;
using System.Windows.Input;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class SystemSettingsViewModel : INotifyPropertyChanged, ITabViewModel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int IpOne { get; set; } = 192;
        public int IpTwo { get; set; } = 168;
        public int IpThree { get; set; } = 1;
        public int IpFour { get; set; } = 2;
        public int PortNumber { get; set; } = 8180;
        public string Name { get; set; } = "System Settings";
        public string ImageSource { get; } = "/Resources/interface (Freepik).png";
        public string ConnectionStatus { get; set; } = "Disconnected";
        public List<SettingsItem> SettingsItems { get; private set; } = new List<SettingsItem>();
        public SettingsItem SelectedSettingsItem { get; set; }
        public ICommand ConnectToWifiCommand { get; }

        private readonly ConfigurationService configurationService;
        private bool connectedToDevice;
        private readonly IDisposable roasterConnectionSubscription;
        private readonly RoasterConnection roasterConnection;

        public SystemSettingsViewModel(RoasterConnection roasterConnection)
        {
            var systemConnectionSetting = new SettingsItem
            {
                SettingName = "Connection",
                SettingIconName = "/Resources/wifi (Roundicons).png"
            };

            var roastDbSettings = new SettingsItem
            {
                SettingName = "Roast Db",
                SettingIconName = "/Resources/database (Pixel perfect).png"
            };

            var roastServerSettings = new SettingsItem
            {
                SettingName = "Server Settings",
                SettingIconName = "/Resources/hardware (Payungkead).png"
            };

            SettingsItems.Add(systemConnectionSetting);
            SettingsItems.Add(roastDbSettings);
            SettingsItems.Add(roastServerSettings);

            SelectedSettingsItem = SettingsItems.First();

            roasterConnectionSubscription = roasterConnection.WiFiConnectionChanged.ObserveOnDispatcher().Subscribe(UpdateConncectionStatus);
            configurationService = new ConfigurationService();
            GetConfigurationData();
            PropertyChanged += SystemSettingsViewModel_PropertyChanged;
            ConnectToWifiCommand = new DelegateCommand(ConnectoToRoaster);
            this.roasterConnection = roasterConnection;
        }

        private void UpdateConncectionStatus(bool connected)
        {
            ConnectionStatus = connected ? "Connected" : "Disconnected";
            connectedToDevice = connected;
        }

        private void SystemSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void ConnectoToRoaster()
        {
            var ipaddress = $"{IpOne}.{IpTwo}.{IpThree}.{IpFour}";

            var couldUpdateConfiguration = configurationService.UpdateConfiguration(ipaddress, PortNumber);

            if (couldUpdateConfiguration && !connectedToDevice)
                roasterConnection.ConnectToDevice();
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