namespace Itminus.Tags.ModbusTcp;

/// <summary>
/// ModbusTcp 通道配置项
/// </summary>
public class ModbusTcpItem
{
    /// <summary>
    /// Ip 地址，默认值为 localhost
    /// </summary>
    public string IpAddr { get; set; } = "localhost";

    /// <summary>
    /// 端口，默认值为 502
    /// </summary>
    public int Port { get; set; } = 502;

    /// <summary>
    /// 读超时
    /// </summary>
    public int ReadTimeout { get; set; } = 10000;

    /// <summary>
    /// 写超时
    /// </summary>
    public int WriteTimeout { get; set; } = 10000;

    /// <summary>
    /// 连接超时
    /// </summary>
    public int ConnTimeout { get; set; } = 1000;
}
