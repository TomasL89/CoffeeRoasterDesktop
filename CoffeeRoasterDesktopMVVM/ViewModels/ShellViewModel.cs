using Caliburn.Micro;
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

		public ShellViewModel()
		{
			Tabs = new ObservableCollection<ITabViewModel>
			{
				new RoastViewModel(),
				new ProfileSetupViewModel(),
				new SystemSettingsViewModel()
			};
		}

	}
}
