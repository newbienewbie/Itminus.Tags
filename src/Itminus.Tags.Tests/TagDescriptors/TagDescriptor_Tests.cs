using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.TagDescriptors;

public class TagDescriptor_Tests
{
    public TagDescriptor_Tests()
    {
    }

    [Fact]
    public void Test_ConversionWithXElement()
    {
        var descriptor1 = new TagDescriptor 
        {
            Address = "DB200.100.1",
            TagKind = TagKinds.BIT,
            TagName = "拍照-请求-标志",
            EndianKind = EndianKinds.LittleEndian,
            TagSize = 1,
            AccessMode = TagAccessMode.RW,
            Note = null,
        };
        var ele1 = descriptor1.ToXElement();
        var xml1 = ele1.ToString();

        var descriptor2 = ele1.ToTagDescriptor();
        var ele2 = descriptor2.ToXElement();
        var xml2 = ele2.ToString();
        Assert.Equal(xml1,xml2);

        Assert.Equal(descriptor1.TagName, descriptor2.TagName);
        Assert.Equal(descriptor1.Address, descriptor2.Address);
        Assert.Equal(descriptor1.TagKind, descriptor2.TagKind);
        Assert.Equal(descriptor1.EndianKind, descriptor2.EndianKind);
        Assert.Equal(descriptor1.TagSize, descriptor2.TagSize);
        Assert.Equal(descriptor1.AccessMode, descriptor2.AccessMode);
        Assert.Equal(descriptor1.Note, descriptor2.Note);
    }

}
