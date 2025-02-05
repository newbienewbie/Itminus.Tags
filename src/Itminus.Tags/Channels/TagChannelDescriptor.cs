using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Itminus.Tags;


/// <summary>
/// <see cref="ITagChannel"/> 描述符
/// </summary>
public class TagChannelDescriptor
{

    /// <summary>
    /// Channel Name
    /// </summary>
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Channel Driver
    /// </summary>
    public virtual string Driver { get; set; } = string.Empty;


    [JsonExtensionData]
    public IDictionary<string, JsonElement> Extras { get; set; } = new Dictionary<string, JsonElement>();
}

