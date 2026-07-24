using Xunit;

namespace Itminus.Tags.Tests.TagGrps;

public class TagGrpExtensionsTests
{
    #region SearchScanInterval

    [Fact]
    public void SearchScanInterval_WhenDescriptorHasValue_ReturnsOwnValue()
    {
        var grp = new TagGrp(new TagGrpDescriptor { Name = "test", ScanInterval = 200 }, channel: null);

        var result = grp.SearchScanInterval();

        Assert.Equal(200, result);
    }

    [Fact]
    public void SearchScanInterval_WhenDescriptorHasZero_ReturnsZero()
    {
        var grp = new TagGrp(new TagGrpDescriptor { Name = "test", ScanInterval = 0 }, channel: null);

        var result = grp.SearchScanInterval();

        Assert.Equal(0, result);
    }

    [Fact]
    public void SearchScanInterval_WhenDescriptorNull_ReturnsNull()
    {
        // Descriptor 本身为 null（极端边缘情况），无父级 → null
        var grp = new TagGrp(new TagGrpDescriptor { Name = "test", ScanInterval = 0 }, channel: null)
        {
            Descriptor = null,
        };

        var result = grp.SearchScanInterval();

        Assert.Null(result);
    }

    [Fact]
    public void SearchScanInterval_BubblesUpMultiLevel()
    {
        var root = new TagGrp(new TagGrpDescriptor { Name = "root", ScanInterval = 500 }, channel: null);
        var mid = new TagGrp(new TagGrpDescriptor { Name = "mid" }, channel: null) { Descriptor = null, Parent = root };
        var leaf = new TagGrp(new TagGrpDescriptor { Name = "leaf" }, channel: null) { Descriptor = null, Parent = mid };

        var result = leaf.SearchScanInterval();

        Assert.Equal(500, result);
    }

    [Fact]
    public void SearchScanInterval_DescriptorExistsButScanIntervalNull_BubblesUp()
    {
        // Descriptor 存在但 ScanInterval 为 null → 冒泡到父级
        var parent = new TagGrp(new TagGrpDescriptor { Name = "parent", ScanInterval = 200 }, channel: null);
        var child = new TagGrp(new TagGrpDescriptor { Name = "child" }, channel: null)
        {
            Parent = parent,
        };

        var result = child.SearchScanInterval();

        Assert.Equal(200, result);
    }

    #endregion
}
