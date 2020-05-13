using Caliburn.Micro;
using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopBackgroundLibrary.Error;
using CoffeeRoasterDesktopUI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoffeeRoasterDesktopMVVM.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private readonly RoasterConnection roasterConnection;
        public ObservableCollection<ITabViewModel> Tabs { get; private set; }
        public ITabViewModel SelectedTab { get; set; }

        public ShellViewModel()
        {
            try
            {
                roasterConnection = RoasterConnection.GetConnectionInstance();
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Connection, $"Class {typeof(ShellViewModel)} failed when attempting to get a connection.", ex);
            }

            Tabs = new ObservableCollection<ITabViewModel>
            {
                new RoastViewModel(roasterConnection),
                new ProfileSetupViewModel(),
                new SystemSettingsViewModel(roasterConnection)
            };

            SelectedTab = Tabs.First();
        }
    }
}