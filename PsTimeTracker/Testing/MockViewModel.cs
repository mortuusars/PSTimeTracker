using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PSTimeTracker.Models;

namespace PSTimeTracker.Testing
{
    public class MockViewModel
    {

        public ObservableCollection<PsFile> Files { get; set; } = new ObservableCollection<PsFile>()
        {
            new PsFile() { FileName = "DSC9123asdasdasdasdasdasd3", TrackedSeconds = 241, IsCurrentlyActive = true},
            new PsFile() { FileName = "DSC91233", TrackedSeconds = 241 },
            new PsFile() { FileName = "DSC91233", TrackedSeconds = 999, IsSelected = true },
            new PsFile() { FileName = "DSC91233", TrackedSeconds = 241 }
        };
    }
}
