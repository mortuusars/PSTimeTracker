using PSTimeTracker.PsTracking;

namespace PSTimeTracker
{
    public class MainViewState
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool AlwaysOnTop { get; set; }
        public Sorting SortingOrder { get; set; }
    }
}
