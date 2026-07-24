using System;
using System.Reflection;
using System.Xml.Linq;
using Itminus.Tags.ModbusTcp;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class ModbusTcpChannelFactoryTests
{
    [Fact]
    public void GetAvailableDrivers_ReturnsModbusTcp()
    {
        var factory = new ModbusTcpChannelFactory(NullLoggerFactory.Instance);

        var drivers = factory.GetAvailableDrivers();

        Assert.Single(drivers);
        Assert.Equal("ModbusTcp", drivers[0]);
    }

    [Fact]
    public void Create_WithDescriptor_ReturnsChannel()
    {
        var factory = new ModbusTcpChannelFactory(NullLoggerFactory.Instance);
        var descriptor = new TagChannelDescriptor
        {
            Name = "mb1",
            Driver = "ModbusTcp",
        };

        var channel = factory.Create(descriptor);

        Assert.NotNull(channel);
        Assert.IsType<ModbusTcpChannel>(channel);
        Assert.Equal("mb1", channel.ChannelName);
        Assert.Equal("ModbusTcp", channel.Driver);
    }

    [Fact]
    public void Create_WithExtras_SetsIpAndPort()
    {
        var factory = new ModbusTcpChannelFactory(NullLoggerFactory.Instance);
        var descriptor = new TagChannelDescriptor
        {
            Name = "mb2",
            Driver = "ModbusTcp",
        };
        descriptor.Extras["IpAddr"] = new XElement("IpAddr", "10.0.0.1");
        descriptor.Extras["Port"] = new XElement("Port", "5021");

        var channel = factory.Create(descriptor);

        Assert.NotNull(channel);
        var mbChannel = Assert.IsType<ModbusTcpChannel>(channel);
        Assert.Equal("10.0.0.1", mbChannel.IpAddr);
        Assert.Equal(5021, mbChannel.Port);
    }

    [Fact]
    public void Create_WithWrongDriver_Throws()
    {
        var factory = new ModbusTcpChannelFactory(NullLoggerFactory.Instance);
        var descriptor = new TagChannelDescriptor
        {
            Name = "bad",
            Driver = "S7",
        };

        Assert.Throws<InvalidOperationException>(() => factory.Create(descriptor));
    }

    [Fact]
    public void Create_WithMaxBatchSize_PropagatesToChannel()
    {
        var factory = new ModbusTcpChannelFactory(NullLoggerFactory.Instance);
        var descriptor = new TagChannelDescriptor
        {
            Name = "mb3",
            Driver = "ModbusTcp",
        };
        descriptor.Extras["MaxBatchSize"] = new XElement("MaxBatchSize", "50");

        var channel = factory.Create(descriptor);

        var mbChannel = Assert.IsType<ModbusTcpChannel>(channel);
        Assert.Equal((ushort)50, mbChannel.MaxBatchSize);
    }
}
