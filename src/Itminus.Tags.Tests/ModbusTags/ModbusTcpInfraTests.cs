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

    #region MaxReadRegisters

    [Fact]
    public void MaxReadRegisters_RoundtripsViaXml()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
            MaxReadRegisters = 100,
        };

        var xml = descriptor.ToXElement();
        var baseDesc = xml.ToTagChannelDescriptor();
        var restored = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal((ushort)100, restored.MaxReadRegisters);
    }

    [Fact]
    public void MaxReadRegisters_DefaultNull()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };

        Assert.Null(descriptor.MaxReadRegisters);
    }

    [Fact]
    public void MaxReadRegisters_ReadFromExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadRegisters"] = new XElement("MaxReadRegisters", "32");

        var result = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal((ushort)32, result.MaxReadRegisters);
    }

    [Fact]
    public void MaxReadRegisters_InvalidValue_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadRegisters"] = new XElement("MaxReadRegisters", "not-a-number");

        Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
    }

    [Fact]
    public void MaxReadRegisters_Zero_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadRegisters"] = new XElement("MaxReadRegisters", "0");

        Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
    }

    [Fact]
    public void MaxReadRegisters_ExceedsProtocolLimit_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadRegisters"] = new XElement("MaxReadRegisters", "126");

        var ex = Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
        Assert.Contains("125", ex.Message);
    }

    #endregion

    #region MaxReadBits

    [Fact]
    public void MaxReadBits_RoundtripsViaXml()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
            MaxReadBits = 500,
        };

        var xml = descriptor.ToXElement();
        var baseDesc = xml.ToTagChannelDescriptor();
        var restored = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal((ushort)500, restored.MaxReadBits);
    }

    [Fact]
    public void MaxReadBits_DefaultNull()
    {
        var descriptor = new ModbusTcpTagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };

        Assert.Null(descriptor.MaxReadBits);
    }

    [Fact]
    public void MaxReadBits_ReadFromExtras()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadBits"] = new XElement("MaxReadBits", "1000");

        var result = baseDesc.ToModbusTcpTagChannelDescriptor();

        Assert.Equal((ushort)1000, result.MaxReadBits);
    }

    [Fact]
    public void MaxReadBits_InvalidValue_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadBits"] = new XElement("MaxReadBits", "not-a-number");

        Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
    }

    [Fact]
    public void MaxReadBits_Zero_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadBits"] = new XElement("MaxReadBits", "0");

        Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
    }

    [Fact]
    public void MaxReadBits_ExceedsProtocolLimit_Throws()
    {
        var baseDesc = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };
        baseDesc.Extras["MaxReadBits"] = new XElement("MaxReadBits", "2001");

        var ex = Assert.Throws<ArgumentException>(() => baseDesc.ToModbusTcpTagChannelDescriptor());
        Assert.Contains("2000", ex.Message);
    }

    #endregion
}
