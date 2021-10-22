namespace PSTimeTracker.ViewModels
{
    public class UpdateViewModel
    {
        public string VersionText { get; set; }
        public string Description { get; set; }

        public UpdateViewModel(string description, string versionText)
        {
            VersionText = versionText;
            Description = description;
        }
    }
}
