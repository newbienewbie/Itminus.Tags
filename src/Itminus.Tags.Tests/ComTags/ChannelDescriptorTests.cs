using Itminus.Tags.ComScanner;
using Itminus.Tags.ComScanner.Channels;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.OpcUaClient;
using Itminus.Tags.S7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        var element2 = descriptor1.ToXElement();
        var xml1 = element.ToString();
        var xml2 = element2.ToString();
        Assert.Equal(xml1, xml2);
    }
}
