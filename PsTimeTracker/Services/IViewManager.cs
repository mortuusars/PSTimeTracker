namespace PSTimeTracker.Services
{
    public interface IViewManager
    {
        public void ShowMainView();
        public void MinimizeMainView();
        public void CloseMainView();

        public void ShowConfigView();
        public void CloseConfigView();
    }
}