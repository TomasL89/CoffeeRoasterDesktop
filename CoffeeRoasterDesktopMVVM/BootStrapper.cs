using Caliburn.Micro;
using CoffeeRoasterDesktopMVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CoffeeRoasterDesktopMVVM
{
    public class BootStrapper : BootstrapperBase
    {
        public BootStrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
