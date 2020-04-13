using CoffeeRoasterDesktopBackground;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows.Input;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class SystemSettingsViewModel : INotifyPropertyChanged, ITabViewModel
    {
        public int IpOne { get; set; } = 192;
        public int IpTwo { get; set; } = 168;
        public int IpThree { get; set; } = 1;
        public int IpFour { get; set; } = 2;
        public int PortNumber { get; set; } = 8180;
        public string Name { get; set; } = "System Settings";
        public ICommand OnWifiConnectPressed { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConfigurationService configurationService;
        private Configuration configuration;

        public SystemSettingsViewModel()
        {
            configurationService = new ConfigurationService();
            configuration = configurationService.SystemConfiguration;
            if (configuration != null)
            {
                GetConfigurationData();
                PropertyChanged += SystemSettingsViewModel_PropertyChanged;
                OnWifiConnectPressed = new DelegateCommand(SetConfigurationData);
            }

        }

        private void SystemSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetConfigurationData();
        }

        private void SetConfigurationData()
        {
            var ipaddress = $"{IpOne}.{IpTwo}.{IpThree}.{IpFour}";

            var newConfiguration = new Configuration
            {
                IpAddress = ipaddress,
                PortNumber = PortNumber,
                LogFileDatabaseDirectory = configuration.LogFileDatabaseDirectory
            };
            // todo notify user that it has been saved
            var couldSaveConfiguration = configurationService.SaveConfiguration(newConfiguration);
        }

        private void GetConfigurationData()
        {
            var ipaddress = configuration.IpAddress.Split(".");
            int.TryParse(ipaddress[0], out int ipOne);
            int.TryParse(ipaddress[1], out int ipTwo);
            int.TryParse(ipaddress[2], out int ipThree);
            int.TryParse(ipaddress[3], out int ipFour);

            IpOne = ipOne;
            IpTwo = ipTwo;
            IpThree = ipThree;
            IpFour = ipFour;

            PortNumber = configuration.PortNumber;
        }
    }
}
