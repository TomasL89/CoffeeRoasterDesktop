using Caliburn.Micro;
using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopUI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoffeeRoasterDesktopMVVM.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        public ObservableCollection<ITabViewModel> Tabs { get; private set; }
        public ITabViewModel SelectedTab { get; set; }
        private readonly RoasterConnection roasterConnection;

        public ShellViewModel()
        {
            roasterConnection = new RoasterConnection();

            try
            {
                roasterConnection.ConnectToDevice();
            }
            catch (Exception)
            {
            }

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new RoastViewModel(roasterConnection),
                new ProfileSetupViewModel(roasterConnection),
                new SystemSettingsViewModel(roasterConnection)
            };

            SelectedTab = Tabs.First();
        }
    }
}