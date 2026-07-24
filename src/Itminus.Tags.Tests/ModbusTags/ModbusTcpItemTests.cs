using Itminus.Tags.ModbusTcp;
using Xunit;

namespace Itminus.Tags.Tests.ModbusTags;

public class ModbusTcpItemTests
{
    [Fact]
    public void DefaultValues()
    {
        var item = new ModbusTcpItem();

        Assert.Equal("localhost", item.IpAddr);
        Assert.Equal(502, item.Port);
        Assert.Equal(10000, item.ReadTimeout);
        Assert.Equal(10000, item.WriteTimeout);
        Assert.Equal(1000, item.ConnTimeout);
    }

    [Fact]
    public void Properties_Roundtrip()
    {
        var item = new ModbusTcpItem
        {
            IpAddr = "192.168.1.100",
            Port = 5020,
            ReadTimeout = 5000,
            WriteTimeout = 5000,
            ConnTimeout = 2000,
        };

        Assert.Equal("192.168.1.100", item.IpAddr);
        Assert.Equal(5020, item.Port);
        Assert.Equal(5000, item.ReadTimeout);
        Assert.Equal(5000, item.WriteTimeout);
        Assert.Equal(2000, item.ConnTimeout);
    }
}
