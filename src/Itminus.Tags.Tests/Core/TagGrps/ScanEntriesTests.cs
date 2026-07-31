using System.Linq;
using Xunit;

namespace Itminus.Tags.Tests.Core.TagGrps;

public class ScanEntriesTests
{
    [Fact]
    public void ScanEntries_WhenSelfIsEntry_ReturnsSelf()
    {
        var grp = new TagGrp(new TagGrpDescriptor { Name = "entry", IsEntry = true }, null);

        var entries = grp.ScanEntries();

        Assert.Single(entries);
        Assert.Same(grp, entries[0]);
    }

    [Fact]
    public void ScanEntries_WhenChildIsEntry_ReturnsChild()
    {
        var root = new TagGrp(new TagGrpDescriptor { Name = "root" }, null);
        var child = new TagGrp(new TagGrpDescriptor { Name = "child", IsEntry = true }, null);
        root.AddTag(child);

        var entries = root.ScanEntries();

        Assert.Single(entries);
        Assert.Same(child, entries[0]);
    }

    [Fact]
    public void ScanEntries_WhenNonEntryChildHasEntryGrandchild_ReturnsGrandchild()
    {
        var root = new TagGrp(new TagGrpDescriptor { Name = "root" }, null);
        var mid = new TagGrp(new TagGrpDescriptor { Name = "mid" }, null);  // 不是入口
        var leaf = new TagGrp(new TagGrpDescriptor { Name = "leaf", IsEntry = true }, null);
        mid.AddTag(leaf);
        root.AddTag(mid);

        var entries = root.ScanEntries();

        Assert.Single(entries);
        Assert.Same(leaf, entries[0]);
    }

    [Fact]
    public void ScanEntries_WhenMultipleChildrenAreEntries_ReturnsAll()
    {
        var root = new TagGrp(new TagGrpDescriptor { Name = "root" }, null);
        var a = new TagGrp(new TagGrpDescriptor { Name = "a", IsEntry = true }, null);
        var b = new TagGrp(new TagGrpDescriptor { Name = "b", IsEntry = true }, null);
        root.AddTag(a);
        root.AddTag(b);

        var entries = root.ScanEntries();

        Assert.Equal(2, entries.Count);
        Assert.Contains(a, entries);
        Assert.Contains(b, entries);
    }



    [Fact]
    public void ScanEntries_RecursesIntoNonEntryChildren()
    {
        /*
            root
            ├─ mid (not entry)
            │  ├─ leaf1 (entry)
            │  └─ leaf2 (entry)
        */
        var root = new TagGrp(new TagGrpDescriptor { Name = "root" }, null);
        var mid = new TagGrp(new TagGrpDescriptor { Name = "mid" }, null);  // 非入口
        var leaf1 = new TagGrp(new TagGrpDescriptor { Name = "leaf1", IsEntry = true }, null);
        var leaf2 = new TagGrp(new TagGrpDescriptor { Name = "leaf2", IsEntry = true }, null);
        mid.AddTag(leaf1);
        mid.AddTag(leaf2);
        root.AddTag(mid);

        var entries = root.ScanEntries();

        Assert.Equal(2, entries.Count);
        Assert.Contains(leaf1, entries);
        Assert.Contains(leaf2, entries);
    }


    [Fact]
    public void ScanEntries_WhenNoChildren_ReturnsEmpty()
    {
        var root = new TagGrp(new TagGrpDescriptor { Name = "root" }, null);
        var entries = root.ScanEntries();
        Assert.Empty(entries);
    }

    [Fact]
    public void ScanEntries_WhenNoEntryExists_ReturnsEmpty()
    {
        var root = new TagGrp(new TagGrpDescriptor { Name = "root" }, null);
        var mid = new TagGrp(new TagGrpDescriptor { Name = "mid" }, null);
        root.AddTag(mid);

        var entries = root.ScanEntries();

        Assert.Empty(entries);
    }
}
