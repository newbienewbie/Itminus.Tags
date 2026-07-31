
using Itminus.Tags;
using Itminus.Tags.SimpleFiles;

namespace NixMonitor.Tags.SimpleTags;


record MyJson(int A, string S);

class MyJsonTag : SimpleFilesDirectTagBase<MyJson>
{
    public MyJsonTag(TagDescriptor descriptor, SimpleFilesTagChannel? thisChannel, TagContainer container)
     : base(descriptor, thisChannel, container)
    {
    }

    protected override MyJson ParseValue(string text)
    {
        return System.Text.Json.JsonSerializer.Deserialize<MyJson>(text) ?? throw new InvalidOperationException("Failed to deserialize JSON.");
    }

    protected override string FormatValue(MyJson? value)
    {
        return System.Text.Json.JsonSerializer.Serialize(value);
    }
}