using Caliburn.Micro;
using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoffeeRoasterDesktopMVVM.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
		public ObservableCollection<ITabViewModel> Tabs { get; private set; }
		private readonly RoasterConnection roasterConnection;

		public ShellViewModel()
		{
			roasterConnection = new RoasterConnection();

			Tabs = new ObservableCollection<ITabViewModel>
			{
				new RoastViewModel(roasterConnection),
				new ProfileSetupViewModel(roasterConnection),
				new SystemSettingsViewModel(roasterConnection)
			};
		}

	}
}
