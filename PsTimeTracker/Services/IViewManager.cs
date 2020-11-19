namespace PSTimeTracker.Services
{
    public interface IViewManager
    {
        public void ShowMainView();

        public void ShowConfigView();
        public void CloseConfigView();
    }
}