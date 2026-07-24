using System;
using System.Xml.Linq;
using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class ModbusTcpTagChannelDescriptorTests
{
    [Fact]
    public void ToXElement_Roundtrips()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
            IpAddr = "192.168.1.10",
            Port = 502,
        };

        var xml = descriptor.ToXElement();
        var baseDesc = xml.ToTagChannelDescriptor();
        var restored = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal("mb1", restored.Name);
        Assert.Equal("ModbusTcp", restored.Driver);
        Assert.Equal("192.168.1.10", restored.IpAddr);
        Assert.Equal(502, restored.Port);
    }

    [Fact]
    public void ToModbusTcpTagChannelDescriptor_FromBaseType_ReadsExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["IpAddr"] = new XElement("IpAddr", "10.0.0.1");
        baseDesc.Extras["Port"] = new XElement("Port", "502");

        var result = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal("10.0.0.1", result.IpAddr);
        Assert.Equal(502, result.Port);
    }

    [Fact]
    public void ToModbusTcpTagChannelDescriptor_FromBaseType_DefaultValues()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };

        var result = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal("localhost", result.IpAddr);
        Assert.Equal(502, result.Port);
    }

    [Fact]
    public void ToModbusTcpTagChannelDescriptor_WithInvalidPort_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["Port"] = new XElement("Port", "not-a-number");

        Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
    }

    [Fact]
    public void ToXElement_ContainsIpAddrAndPort()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
            IpAddr = "10.0.0.5",
            Port = 502,
        };

        var xml = descriptor.ToXElement().ToString();

        Assert.Contains("IpAddr", xml);
        Assert.Contains("10.0.0.5", xml);
        Assert.Contains("Port", xml);
        Assert.Contains("502", xml);
    }
}
