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

        public string SelectedInfo { get; private set; } = string.Empty;

        public IList? SelectedFiles { get; set; }

        public ICommand OpenConfigCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand RemoveFilesCommand { get; }
        public ICommand MergeCommand { get; }
        public ICommand AddDebugFileCommand { get; }

        public ICommand SelectionChangedCommand { get; }

        private readonly ViewManager _viewManager;

        public MainViewViewModel(ITrackingHandler trackingHandler, ViewManager viewManager)
        {
            TrackingHandler = trackingHandler;

            _viewManager = viewManager;

            OpenConfigCommand = new RelayCommand(_ => _viewManager.ShowConfigView());
            OpenAboutCommand = new RelayCommand(_ => _viewManager.ShowAboutView());

            RemoveFilesCommand = new RelayCommand(_ => RemoveFiles(SelectedFiles));
            MergeCommand = new RelayCommand(item => MergeFiles(item, SelectedFiles));

            AddDebugFileCommand = new RelayCommand(_ => AddDebugFile());

            SelectionChangedCommand = new RelayCommand(_ => UpdateSelectedInfo(SelectedFiles));

            TrackingHandler.StartTrackingAsync();
        }

        private void AddDebugFile()
        {
            TrackingHandler.TrackedFiles.Add(new TrackedFile()
            {
                FileName = new Random().Next().ToString(),
                TrackedSeconds = new Random().Next(0, 4999)
            });
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

        private void UpdateSelectedInfo(IList? selectedFiles)
        {
            if (selectedFiles is null)
                return;

            int selectedSummary = 0;
            int count = selectedFiles.Count;

            foreach (var file in selectedFiles)
            {
                selectedSummary += ((TrackedFile)file).TrackedSeconds;
            }

            SelectedInfo = $"{TimeFormatter.GetTimeStringFromSeconds(selectedSummary)} | {count} file/s";
        }
    }
}