using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itminus.Tags;
using Xunit;

namespace Itminus.Tags.Tests.TagUnions;

public class TagUnionExtensions_IsDirty_Tests
{
    private sealed class FakeTag : ITag
    {
        public TagDescriptor TagDescriptor { get; set; } = new() { TagName = "t" };
        public object? Value { get; set; }
        public DateTime Timestamp { get; set; }

        public event TagSyncEventHandler OnTagRead { add { } remove { } }
        public event TagSyncEventHandler OnTagWritten { add { } remove { } }

        public bool IsScaned { get; set; }
        public bool IsDirty { get; set; }
        public ITagChannel? Channel => null;

        public TagContainer? Parent { get ; set; }

        public Task ReadAsync(CancellationToken ct) => Task.CompletedTask;
        public Task WriteAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakeCbnt : ITagCbnt
    {
        public string Name { get; set; } = "cbnt";
        public ITagGrp? Parent { get; set; }

        private readonly Dictionary<string, ITagCbntor> _children = new();
        public IDictionary<string, ITagCbntor> Children => _children;
        public ITagCbntor this[string key] => _children[key];

        public int ScanInterval { get; set; }
        public bool IsEnabled { get; set; } = true;
        public TagAccessMode? AcessMode { get; set; }
        public bool IsScaned { get; set; }
        public ITagChannel? Channel { get; set; }
        public string StartAddress { get; set; } = string.Empty;

        public Memory<byte> Cache { get; } = Memory<byte>.Empty;
        public int CacheSize => 0;
        public void ResizeCache(int cacheSize) { }

        public bool IsDirty { get; set; }
        public Task ReadAsync(CancellationToken ct) => Task.CompletedTask;
        public Task WriteAsync(CancellationToken ct) => Task.CompletedTask;
    }


    [Fact]
    public void IsDirty_For_TagUnit_Uses_Tag_IsDirty()
    {
        var tag = new FakeTag { IsDirty = true };
        var u = new TagUnion.TagUnit(tag);
        Assert.True(u.IsDirty());

        tag.IsDirty = false;
        Assert.False(u.IsDirty());
    }

    [Fact]
    public void IsDirty_For_TagCbnt_Uses_Cbnt_IsDirty()
    {
        var cbnt = new FakeCbnt { IsDirty = true };
        var u = new TagUnion.TagCbnt(cbnt);
        Assert.True(u.IsDirty());

        cbnt.IsDirty = false;
        Assert.False(u.IsDirty());
    }

    [Fact]
    public void IsDirty_For_TagGrp_Uses_Grp_IsDirty_Method()
    {
        var grp = new TagGrp(name: "grp1", isEntry: true, channel: null);
        var u = new TagUnion.TagGrp(grp);
        Assert.False(u.IsDirty());
        grp.AddTag(new FakeTag { IsDirty = true });
        Assert.True(u.IsDirty());
    }

    [Fact]
    public void IsDirty_For_TagGrp_Nested_Any_Dirty_Makes_Root_Dirty()
    {
        // root
        var root = new TagGrp(name: "root", isEntry: true, channel: null);
        var rootUnion = new TagUnion.TagGrp(root);

        // level1 groups
        var g1 = new TagGrp(name: "g1", isEntry: false, channel: null);
        var g2 = new TagGrp(name: "g2", isEntry: false, channel: null);
        root.AddTag(g1);
        root.AddTag(g2);

        // level2 under g1
        var g1_1 = new TagGrp(name: "g1_1", isEntry: false, channel: null);
        g1.AddTag(g1_1);

        // tags in various places
        var rootTag = new FakeTag { TagDescriptor = new() { TagName = "t_root" }, IsDirty = false };
        var g1Tag = new FakeTag { TagDescriptor = new() { TagName = "t_g1" }, IsDirty = false };
        var deepTag = new FakeTag { TagDescriptor = new() { TagName = "t_deep" }, IsDirty = false };
        var g2Tag = new FakeTag { TagDescriptor = new() { TagName = "t_g2" }, IsDirty = false };
        root.AddTag(rootTag);
        g1.AddTag(g1Tag);
        g1_1.AddTag(deepTag);
        g2.AddTag(g2Tag);

        // baseline
        Assert.False(rootUnion.IsDirty());

       
        deepTag.IsDirty = true;
        Assert.True(rootUnion.IsDirty());
        deepTag.IsDirty = false;
        Assert.False(rootUnion.IsDirty());


        g1Tag.IsDirty = true;
        Assert.True(rootUnion.IsDirty());
        g1Tag.IsDirty = false;
        Assert.False(rootUnion.IsDirty());

        g2Tag.IsDirty = true;
        Assert.True(rootUnion.IsDirty());
        g2Tag.IsDirty = false;
        Assert.False(rootUnion.IsDirty());

        rootTag.IsDirty = true;
        Assert.True(rootUnion.IsDirty());
    }
}
