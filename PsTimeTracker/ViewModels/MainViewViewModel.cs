using System.Windows.Input;
using PSTimeTracker.Services;
using PropertyChanged;
using System.Diagnostics;
using System;
using PSTimeTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PSTimeTracker.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewViewModel
    {
        public ITrackingHandler TrackingHandler { get; }

        //public bool IsRestoreAvailable { get; private set; }

        public ICommand OpenConfigCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand RemoveFilesCommand { get; }
        public ICommand MergeCommand { get; }

        private readonly ViewManager _viewManager;

        public MainViewViewModel(ITrackingHandler trackingHandler, ViewManager viewManager)
        {
            TrackingHandler = trackingHandler;

            _viewManager = viewManager;

            OpenConfigCommand = new RelayCommand(_ => _viewManager.ShowConfigView());
            OpenAboutCommand = new RelayCommand(_ => _viewManager.ShowAboutView());

            RemoveFilesCommand = new RelayCommand(files => RemoveFiles(files));
            MergeCommand = new RelayCommand(item => MergeFiles(item));

            TrackingHandler.StartTrackingAsync();
        }

        private void RemoveFiles(object files)
        {
            try
            {
                var list = (System.Collections.IList)files;
                var filesToRemove = list.Cast<TrackedFile>().ToArray();
                TrackingHandler.RemoveFiles(filesToRemove);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove files: \n\n" + ex.Message, "PSTimeTracker", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void MergeFiles(object item)
        {
            throw new NotImplementedException();
        }
    }
}