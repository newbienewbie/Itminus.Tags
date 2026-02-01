using Itminus.Tags.ModbusTcp;
using Itminus.Tags.S7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Itminus.Tags.Tests.Channels;

public class ChannelDescriptorTests
{
    [Fact]
    public void TestLoadFromXml()
    {
        var xml =
@"
<Channel name='S7-3' driver='S7' >
    <IpAddr>192.168.1.13</IpAddr>
    <Rack>3</Rack>
    <Slot>4</Slot>
</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor = TagChannelDescriptor.LoadFromXElement(element);
        Assert.Equal("S7-3", descriptor.Name);
        Assert.Equal("S7", descriptor.Driver);
        Assert.NotEmpty(descriptor.Extras);
        Assert.Equal(3, descriptor.Extras.Count);
        Assert.Equal("192.168.1.13", descriptor.Extras["IpAddr"].Value);
        Assert.Equal("3", descriptor.Extras["Rack"].Value);
        Assert.Equal("4", descriptor.Extras["Slot"].Value);
    }


    [Fact]
    public void TestLoadS7TagChannelDescriptorFromXml()
    {
        var xml =
@"
<Channel name='S7-3' driver='S7' >
    <IpAddr>192.168.1.13</IpAddr>
    <Rack>3</Rack>
    <Slot>4</Slot>
</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = TagChannelDescriptor.LoadFromXElement(element);
        var descriptor1 = S7TagChannelDescriptor.FromDescriptor(descriptor0);
        Assert.Equal("192.168.1.13", descriptor1.IpAddr);
        Assert.Equal(3, descriptor1.Rack);
        Assert.Equal(4, descriptor1.Slot);
    }



    [Fact]
    public void TestLoadModbusTcpTagChannelDescriptorFromXml()
    {
        var xml =
@"
<Channel name='ModbusTcp-1' driver='ModbusTcp' >
    <IpAddr>192.168.1.13</IpAddr>
    <Port>503</Port>
</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = TagChannelDescriptor.LoadFromXElement(element);
        var descriptor1 = ModbusTcpTagChannelDescriptor.FromDescriptor(descriptor0);
        Assert.Equal("192.168.1.13", descriptor1.IpAddr);
        Assert.Equal(503, descriptor1.Port);
    }


    [Fact]
    public void TestLoadModbusTcpTagChannelDescriptorFromXml_WithDefaultPort()
    {
        var xml =
@"
<Channel name='ModbusTcp-1' driver='ModbusTcp' >
    <IpAddr>192.168.1.13</IpAddr>
</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = TagChannelDescriptor.LoadFromXElement(element);
        var descriptor1 = ModbusTcpTagChannelDescriptor.FromDescriptor(descriptor0);
        Assert.Equal("192.168.1.13", descriptor1.IpAddr);
        Assert.Equal(502, descriptor1.Port);
    }
}
