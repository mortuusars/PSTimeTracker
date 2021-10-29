using PropertyChanged;
using PSTimeTracker.Models;
using PSTimeTracker.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PSTimeTracker.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewViewModel
    {
        public ITrackingHandler TrackingHandler { get; }

        public IList? SelectedFiles { get; set; }

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

            RemoveFilesCommand = new RelayCommand(_ => RemoveFiles(SelectedFiles));
            MergeCommand = new RelayCommand(item => MergeFiles(item, SelectedFiles));

            TrackingHandler.StartTrackingAsync();
        }

        private void RemoveFiles(IList? files)
        {
            if (files is null)
                return;

            try
            {
                var filesToRemove = files.Cast<TrackedFile>().ToArray();
                TrackingHandler.RemoveFiles(filesToRemove);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove files: \n\n" + ex.Message, "PSTimeTracker", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MergeFiles(object item, IList? selectedFiles)
        {
            if (item is TrackedFile destination && selectedFiles is not null)
            {
                IList<TrackedFile> inputs = selectedFiles.Cast<TrackedFile>().ToList();
                inputs.Remove(destination);

                TrackingHandler.MergeFiles(destination, inputs);
            }
        }
    }
}