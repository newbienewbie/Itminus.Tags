namespace Itminus.Tags.Web.Tags;

public class TagsProjectEventArgs : EventArgs
{
    public TagsProjectEventArgs(bool isstarted, ITagsProject project)
    {
        this.IsStarted = isstarted;
        Project = project;
    }

    public ITagsProject Project { get; }

    public bool IsStarted { get; }
}


public delegate void TagsProjectStartedOrStopped(TagsProjectCtrl sender, TagsProjectEventArgs args);