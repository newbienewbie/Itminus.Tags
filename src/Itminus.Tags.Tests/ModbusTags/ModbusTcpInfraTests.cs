using System;
using System.Xml.Linq;
using Itminus.Tags.ModbusTcp;
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

    #region MaxBatchSize

    [Fact]
    public void MaxBatchSize_RoundtripsViaXml()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
            IpAddr = "10.0.0.1",
            Port = 502,
            MaxBatchSize = 50,
        };

        var xml = descriptor.ToXElement();
        var baseDesc = xml.ToTagChannelDescriptor();
        var restored = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal((ushort)50, restored.MaxBatchSize);
    }

    [Fact]
    public void MaxBatchSize_DefaultNull()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };

        Assert.Null(descriptor.MaxBatchSize);
    }

    [Fact]
    public void MaxBatchSize_ReadFromExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxBatchSize"] = new XElement("MaxBatchSize", "30");

        var result = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal((ushort)30, result.MaxBatchSize);
    }

    [Fact]
    public void MaxBatchSize_InvalidValue_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxBatchSize"] = new XElement("MaxBatchSize", "not-a-number");

        Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
    }

    #endregion
}
