namespace Itminus.Tags;

public class TagTraverser : TraversingVisitorBase
{
    private readonly Action<ITag> _action;

    public TagTraverser(Action<ITag> action)
    {
        this._action = action;
    }

    protected override void Process(ITagGrp node) { }

    protected override void Process(ITagCbnt node) { }

    protected override void Process(ITag node)
    {
       this._action.Invoke(node);
    }
}