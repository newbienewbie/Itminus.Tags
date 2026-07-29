using Itminus.Tags.ZLan;
using Itminus.Tags.ModbusTcp;
using System;
using System.Xml.Linq;
using Xunit;

namespace Itminus.Tags.Tests.ZLanTags;

public class ZLanTcpTagChannelDescriptorTests
{
    #region ToXElement / XML Roundtrip

    [Fact]
    public void ToXElement_Roundtrips()
    {
        var descriptor = new ZLanTcpTagChannelDescriptor
        {
            Name = "zlan-1",
            IpAddr = "192.168.1.200",
            Port = 503,
        };

        var xml = descriptor.ToXElement();
        var baseDesc = xml.ToTagChannelDescriptor();
        var restored = baseDesc.ToZLanTcpTagChannelDescriptor();

        Assert.Equal("zlan-1", restored.Name);
        Assert.Equal(ZLanTcpNames.DriverName, restored.Driver);
        Assert.Equal("192.168.1.200", restored.IpAddr);
        Assert.Equal(503, restored.Port);
    }

    [Fact]
    public void ToXElement_WithMaxBatchSize_Roundtrips()
    {
        var descriptor = new ZLanTcpTagChannelDescriptor
        {
            Name = "zlan-batch",
            IpAddr = "10.0.0.1",
            Port = 502,
            MaxBatchSize = 30,
        };

        var xml = descriptor.ToXElement();
        var baseDesc = xml.ToTagChannelDescriptor();
        var restored = baseDesc.ToZLanTcpTagChannelDescriptor();

        Assert.Equal((ushort)30, restored.MaxBatchSize);
    }

    [Fact]
    public void ToXElement_ContainsExpectedElements()
    {
        var descriptor = new ZLanTcpTagChannelDescriptor
        {
            Name = "zlan-xml",
            IpAddr = "10.0.0.5",
            Port = 502,
        };

        var xml = descriptor.ToXElement().ToString();

        Assert.Contains("IpAddr", xml);
        Assert.Contains("10.0.0.5", xml);
        Assert.Contains("Port", xml);
    }

    #endregion

    #region ToZLanTcpTagChannelDescriptor (From Base TagChannelDescriptor via Extras)

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_FromBaseType_ReadsExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "zlan-extras",
            Driver = ZLanTcpNames.DriverName,
        };
        baseDesc.Extras["IpAddr"] = new XElement("IpAddr", "10.0.0.1");
        baseDesc.Extras["Port"] = new XElement("Port", "502");

        var result = baseDesc.ToZLanTcpTagChannelDescriptor();

        Assert.Equal("10.0.0.1", result.IpAddr);
        Assert.Equal(502, result.Port);
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_FromBaseType_DefaultValues()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "zlan-defaults",
            Driver = ZLanTcpNames.DriverName,
        };

        var result = baseDesc.ToZLanTcpTagChannelDescriptor();

        Assert.Equal("localhost", result.IpAddr);
        Assert.Equal(502, result.Port);
        Assert.Null(result.MaxBatchSize);
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_WhenAlreadyZLanDescriptor_ReturnsSame()
    {
        var descriptor = new ZLanTcpTagChannelDescriptor
        {
            Name = "zlan-already",
            IpAddr = "10.0.0.1",
            Port = 502,
        };

        var result = descriptor.ToZLanTcpTagChannelDescriptor();

        Assert.Same(descriptor, result);
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_WrongDriver_Throws()
    {
        var descriptor = new TagChannelDescriptor
        {
            Name = "wrong",
            Driver = "S7",
        };

        var ex = Assert.Throws<InvalidOperationException>(() => descriptor.ToZLanTcpTagChannelDescriptor());
        Assert.Contains(ZLanTcpNames.DriverName, ex.Message);
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_InvalidPort_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "zlan-bad-port",
            Driver = ZLanTcpNames.DriverName,
        };
        baseDesc.Extras["Port"] = new XElement("Port", "not-a-number");

        Assert.Throws<ArgumentException>(() => baseDesc.ToZLanTcpTagChannelDescriptor());
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_InvalidMaxBatchSize_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "zlan-bad-batch",
            Driver = ZLanTcpNames.DriverName,
        };
        baseDesc.Extras["MaxBatchSize"] = new XElement("MaxBatchSize", "not-a-number");

        Assert.Throws<ArgumentException>(() => baseDesc.ToZLanTcpTagChannelDescriptor());
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_MaxBatchSize_ReadsFromExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "zlan-batch-extras",
            Driver = ZLanTcpNames.DriverName,
        };
        baseDesc.Extras["MaxBatchSize"] = new XElement("MaxBatchSize", "50");

        var result = baseDesc.ToZLanTcpTagChannelDescriptor();

        Assert.Equal((ushort)50, result.MaxBatchSize);
    }

    [Fact]
    public void ToZLanTcpTagChannelDescriptor_PreservesExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "zlan-extra-keys",
            Driver = ZLanTcpNames.DriverName,
        };
        baseDesc.Extras["CustomKey"] = new XElement("CustomKey", "CustomValue");

        var result = baseDesc.ToZLanTcpTagChannelDescriptor();

        Assert.True(result.Extras.ContainsKey("CustomKey"));
        Assert.Equal("CustomValue", result.Extras["CustomKey"].Value);
    }

    #endregion
}
