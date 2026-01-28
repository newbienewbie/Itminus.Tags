using System.Text.Json;
using System.Text.Json.Serialization;


namespace Itminus.Tags.Projects;


public static class ChannelsParser
{
    public static IList<ChannelDescriptor> ReadChannels(string indexPath)
    {
        var files = ParseChannelsIndex(indexPath);
        var list = new List<ChannelDescriptor>();
        foreach (var line in files)
        {
            var descriptor = IncludeTagChannelFile(line);
            list.Add(descriptor);
        }
        return list;
    }

    private static IList<string> ParseChannelsIndex(string indexPath)
    {
        if (!File.Exists(indexPath))
        {
            throw new Exception($"指定的通道索引文件路径不存在({indexPath})");
        }

        var dir = Path.GetDirectoryName(indexPath) ?? throw new Exception($"无法获取通道索引文件所在目录");
        var stream = new FileStream(indexPath, FileMode.Open);
        var locations = JsonSerializer.Deserialize<IList<string>>(stream)
            ?? throw new Exception($"非法的通道索引。文件路径={indexPath}");
        return locations.Select(l => Path.Combine(dir, l)).ToList();
    }

    private static ChannelDescriptor IncludeTagChannelFile(string descriptorPath)
    {
        if (!File.Exists(descriptorPath))
        {
            throw new Exception($"指定的通道配置文件路径不存在({descriptorPath})");
        }
        var stream = new FileStream(descriptorPath, FileMode.Open);
        var descriptor = JsonSerializer.Deserialize<ChannelDescriptor>(stream);
        return descriptor ?? throw new Exception($"非法的通道配置。文件路径={descriptorPath}");
    }
}
 