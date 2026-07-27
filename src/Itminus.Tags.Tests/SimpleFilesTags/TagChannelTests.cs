using System.IO;
using System.Threading;
using Itminus.Tags.SimpleFiles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Itminus.Tags.Tests.SimpleFilesTags;

public class TagChannelTests
{


    #region SimpleFilesTagChannel

    [Fact]
    public void Channel_Constructor_SetsProperties()
    {
        var settings = new SimpleFilesSettings { BaseDir = @"C:\base" };
        var logger = NullLogger<SimpleFilesTagChannel>.Instance;
        var channel = new SimpleFilesTagChannel("test-ch", settings, logger);

        Assert.Equal("test-ch", channel.ChannelName);
        Assert.Same(settings, channel.Settings);
        Assert.Equal(SimpleFilesNames.DriverName, channel.Driver);
    }

    [Fact]
    public void Channel_MakePath_WithBaseDir_CombinesPath()
    {
        var settings = new SimpleFilesSettings { BaseDir = @"C:\base" };
        var channel = new SimpleFilesTagChannel("ch", settings, NullLogger<SimpleFilesTagChannel>.Instance);

        var path = channel.MakePath(@"sub\file.txt");

        Assert.Equal(Path.Combine(@"C:\base", @"sub\file.txt"), path);
    }

    [Fact]
    public void Channel_MakePath_WithoutBaseDir_ReturnsAddressAsIs()
    {
        var settings = new SimpleFilesSettings();
        var channel = new SimpleFilesTagChannel("ch", settings, NullLogger<SimpleFilesTagChannel>.Instance);

        var path = channel.MakePath(@"C:\absolute\path.txt");

        Assert.Equal(@"C:\absolute\path.txt", path);
    }

    [Fact]
    public void Channel_EnsureConnectedAsync_DoesNothing()
    {
        var channel = new SimpleFilesTagChannel("ch", new SimpleFilesSettings(), NullLogger<SimpleFilesTagChannel>.Instance);

        var task = channel.EnsureConnectedAsync(false, CancellationToken.None);

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void Channel_DisconnectAsync_DoesNothing()
    {
        var channel = new SimpleFilesTagChannel("ch", new SimpleFilesSettings(), NullLogger<SimpleFilesTagChannel>.Instance);

        var task = channel.DisconnectAsync(CancellationToken.None);

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void Channel_Dispose_DoesNotThrow()
    {
        var channel = new SimpleFilesTagChannel("ch", new SimpleFilesSettings(), NullLogger<SimpleFilesTagChannel>.Instance);

        channel.Dispose(); // should not throw
    }

    #endregion


}
