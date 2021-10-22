using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PSTimeTracker.Models;

namespace PSTimeTracker.Testing
{
    public class MockViewModel
    {
        public ObservableCollection<TrackedFile> Files { get; set; } = new ObservableCollection<TrackedFile>()
        {
            new TrackedFile() { FileName = "DSC91233", TrackedSeconds = 241},
            new TrackedFile() { FileName = "DSC9123asdasdasdasdasdasd3aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", TrackedSeconds = 241 },
            new TrackedFile() { FileName = "DSC9123asdasdasdasdasdasd3aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", TrackedSeconds = 15241 },
            new TrackedFile() { FileName = "DSC91233", TrackedSeconds = 15241, IsCurrentlyActive = true }
        };
    }
}
