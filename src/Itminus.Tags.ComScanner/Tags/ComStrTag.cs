using Itminus.Tags.ComScanner.Channels;
using System.Text;

namespace Itminus.Tags.ComScanner.Tags;

/// <summary>
/// 字符串型COM测点。<br/>
/// </summary>
public class ComStrTag : ComTagBase<string>
{
    public ComStrTag(TagDescriptor descriptor, ComChannelBase<string>? thisChannel, TagContainer container)
        : base(descriptor, thisChannel, container)
    {
    }


    protected override byte[] ConvertValueToBytes(string val) 
        => Encoding.UTF8.GetBytes(val);
}
