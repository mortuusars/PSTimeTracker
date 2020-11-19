using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using PSTimeTracker.Services;

namespace PSTimeTracker.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsMenuOpen { get; set; }

        public ICommand MenuCommand { get; }
        public ICommand OpenPreviousCommand { get; }
        public ICommand OpenConfigCommand { get; }
        public ICommand OpenAboutCommand { get; }

        private readonly IViewManager _viewManager;

        public MenuViewModel(IViewManager viewManager)
        {
            _viewManager = viewManager;

            MenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            OpenConfigCommand = new RelayCommand(_ => OnOpenConfigCommand());
        }


        private void OnOpenConfigCommand()
        {
            IsMenuOpen = false;
            _viewManager.ShowConfigView();
        }
    }
}
