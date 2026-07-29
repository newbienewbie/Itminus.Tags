using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using System.Xml.Linq;
using Xunit;

namespace Itminus.Tags.Tests.ComTags;

public class ChannelDescriptorTests
{





    [Fact]
    public void TestLoadComChannelDescriptorFromXml()
    {
        var xml =
@"
	<Channel name=""COM-2"" driver=""COM"">
		<ChannelCapacity>42</ChannelCapacity>
		<Port>COM2</Port>
		<BaundRate>9600</BaundRate>
		<Parity>None</Parity>
		<DataBits>8</DataBits>
		<StopBits>One</StopBits>
	</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = element.ToTagChannelDescriptor();
        var descriptor1 = descriptor0.ToComChannelDescriptor();
        Assert.Equal(ComDriverNames.DriverName, descriptor1.Driver);
        Assert.Equal("COM-2", descriptor1.Name);
        var opt = descriptor1.Option;

        Assert.Equal(42, opt.ChannelCapacity);
        Assert.Equal("COM2", opt.Port);
        Assert.Equal(9600, opt.BaundRate);
        Assert.Equal(System.IO.Ports.Parity.None, opt.Parity);
        Assert.Equal(8, opt.DataBits);
        Assert.Equal(System.IO.Ports.StopBits.One, opt.StopBits);
        Assert.False(opt.ReadScriptDebugInformationEnabled);
        Assert.True(opt.ReadEntireLine);   // 默认值应为 true


        var element2 = descriptor1.ToXElement();
        var xml1 = element.ToString();
        var xml2 = element2.ToString();
        Assert.Equal(xml1, xml2);
    }


    [Fact]
    public void TestLoadScriptBasedComChannelDescriptorFromXml()
    {
        var xml =
@"
	<Channel name=""COM-2"" driver=""COM"">
		<ChannelCapacity>42</ChannelCapacity>
		<Port>COM2</Port>
		<BaundRate>9600</BaundRate>
		<Parity>None</Parity>
		<DataBits>8</DataBits>
		<StopBits>One</StopBits>
        <ReadScript>return ""1"";</ReadScript>
        <ReadScriptDebugInformationEnabled>true</ReadScriptDebugInformationEnabled>
	</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = element.ToTagChannelDescriptor();
        var descriptor1 = descriptor0.ToComChannelDescriptor();
        Assert.Equal(ComDriverNames.DriverName, descriptor1.Driver);
        Assert.Equal("COM-2", descriptor1.Name);
        var opt = descriptor1.Option;

        Assert.Equal(42, opt.ChannelCapacity);
        Assert.Equal("COM2", opt.Port);
        Assert.Equal(9600, opt.BaundRate);
        Assert.Equal(System.IO.Ports.Parity.None, opt.Parity);
        Assert.Equal(8, opt.DataBits);
        Assert.Equal(System.IO.Ports.StopBits.One, opt.StopBits);
        Assert.Equal("return \"1\";", opt.ReadScript);
        Assert.True(opt.ReadScriptDebugInformationEnabled);
        Assert.True(opt.ReadEntireLine);   // 默认值应为 true


        var element2 = descriptor1.ToXElement();
        var xml1 = element.ToString();
        var xml2 = element2.ToString();
        Assert.Equal(xml1, xml2);
    }


    [Fact]
    public void TestLoadComChannelDescriptorFromXml_WithReadEntireLineFalse()
    {
        var xml =
@"
	<Channel name=""COM-3"" driver=""COM"">
		<ChannelCapacity>10</ChannelCapacity>
		<Port>COM3</Port>
		<BaundRate>115200</BaundRate>
		<Parity>None</Parity>
		<DataBits>8</DataBits>
		<StopBits>One</StopBits>
		<ReadEntireLine>false</ReadEntireLine>
	</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = element.ToTagChannelDescriptor();
        var descriptor1 = descriptor0.ToComChannelDescriptor();
        Assert.Equal(ComDriverNames.DriverName, descriptor1.Driver);
        Assert.Equal("COM-3", descriptor1.Name);
        var opt = descriptor1.Option;

        Assert.Equal(10, opt.ChannelCapacity);
        Assert.Equal("COM3", opt.Port);
        Assert.Equal(115200, opt.BaundRate);
        Assert.Equal(System.IO.Ports.Parity.None, opt.Parity);
        Assert.Equal(8, opt.DataBits);
        Assert.Equal(System.IO.Ports.StopBits.One, opt.StopBits);
        Assert.False(opt.ReadEntireLine);  // 显式 false 应被正确解析

        // 往返一致性检验：false 是非默认值，应被序列化保留
        var element2 = descriptor1.ToXElement();
        var xml1 = element.ToString();
        var xml2 = element2.ToString();
        Assert.Equal(xml1, xml2);
    }


    [Fact]
    public void TestLoadComChannelDescriptorFromXml_WithReadEntireLineTrue()
    {
        var xml =
@"
	<Channel name=""COM-4"" driver=""COM"">
		<ChannelCapacity>20</ChannelCapacity>
		<Port>COM4</Port>
		<BaundRate>9600</BaundRate>
		<Parity>None</Parity>
		<DataBits>8</DataBits>
		<StopBits>One</StopBits>
		<ReadEntireLine>true</ReadEntireLine>
	</Channel>
";
        var element = XElement.Parse(xml);
        var descriptor0 = element.ToTagChannelDescriptor();
        var descriptor1 = descriptor0.ToComChannelDescriptor();
        Assert.Equal(ComDriverNames.DriverName, descriptor1.Driver);
        Assert.Equal("COM-4", descriptor1.Name);
        var opt = descriptor1.Option;

        Assert.Equal(20, opt.ChannelCapacity);
        Assert.Equal("COM4", opt.Port);
        Assert.Equal(9600, opt.BaundRate);
        Assert.Equal(System.IO.Ports.Parity.None, opt.Parity);
        Assert.Equal(8, opt.DataBits);
        Assert.Equal(System.IO.Ports.StopBits.One, opt.StopBits);
        Assert.True(opt.ReadEntireLine);  // 语义正确

        // 往返一致性检验：即使在 XML 中显式指定 true，语义也应保留
        var element2 = descriptor1.ToXElement();
        var descriptor2 = element2.ToTagChannelDescriptor().ToComChannelDescriptor();
        Assert.True(descriptor2.Option.ReadEntireLine);
    }


    [Fact]
    public void TestComChannelDescriptorRoundtrip_PreservesReadEntireLine()
    {
        // 验证：通过描述符创建→ToXElement→ToTagChannelDescriptor→ToComChannelDescriptor
        // 整个链路上 ReadEntireLine 的语义保持不变
        var original = new ComChannelDescriptor
        {
            Name = "COM-Test",
            Driver = ComDriverNames.DriverName,
            Option = new ComChannelOption
            {
                Port = "COM5",
                BaundRate = 9600,
                Parity = System.IO.Ports.Parity.None,
                DataBits = 8,
                StopBits = System.IO.Ports.StopBits.One,
                ChannelCapacity = 5,
                ReadEntireLine = false,
            },
        };

        var xml = original.ToXElement();
        var descriptor0 = xml.ToTagChannelDescriptor();
        var descriptor1 = descriptor0.ToComChannelDescriptor();

        Assert.Equal(original.Option.Port, descriptor1.Option.Port);
        Assert.Equal(original.Option.BaundRate, descriptor1.Option.BaundRate);
        Assert.Equal(original.Option.ReadEntireLine, descriptor1.Option.ReadEntireLine);
    }
}

