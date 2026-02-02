using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.Tags.Tests.TagGrpDescriptors;

public class TagGrpDescriptor_Tests
{
    public TagGrpDescriptor_Tests()
    {
    }

    [Fact]
    public void Test_ConversionWithXElement()
    {
        var grp1 = new TagGrpDescriptor
        {
            Name = "Group1",
            ChannelName = "ChannelA",
            IsEntry = true,
            IsEnabled = true,
            ScanInterval = 1000,
        };

        var cbnt1 = new TagCbntDescriptor
        {
            Name = "TagCbnt1",
            StartAddress = "D100",
            ScanInterval = 500,
            IsEnabled = true,
            AccessMode = TagAccessMode.RW,
        };
        cbnt1.Children.Add(new TagDescriptor
        {
            TagName = "Tag1",
            Address = "DB100.100",
            TagKind = TagKinds.INT16,
            EndianKind = EndianKinds.BigEndian,
            AccessMode = TagAccessMode.RW,
        });
        grp1.Children.Add(cbnt1);

        var cbnt2 = new TagCbntDescriptor
        {
            Name = "Tag2",
            StartAddress = "D200",
            ScanInterval = 500,
            IsEnabled = true,
            AccessMode = TagAccessMode.RO,
        };
        cbnt2.Children.Add(new TagDescriptor
        {
            TagName = "Tag3",
            Address = "DB200.200",
            TagKind = TagKinds.FLOAT,
            EndianKind = EndianKinds.LittleEndian,
            AccessMode = TagAccessMode.RO,
        });
   
        grp1.Children.Add(cbnt2);

        var element1 = grp1.ToXElement();
        var xml1 = element1.ToString();

        var grp2 = element1.ToTagGrpDescriptor();
        var xml2 = grp2.ToXElement().ToString();
        Assert.Equal(xml1, xml2);
    }

}
