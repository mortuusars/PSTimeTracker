using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace PSTimeTracker.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class UpdateViewModel
    {
        public string VersionText { get; set; }
        public string Description { get; set; }
    }
}
