namespace Itminus.Tags.ModbusTcp;

public class ModbusTcpItem
{
    public string IpAddr { get; set; } = "localhost";

    public int Port { get; set; } = 502;

    public int ReadTimeout { get; set; } = 10000;


    public int WriteTimeout { get; set; } = 10000;


    public int ConnTimeout { get; set; } = 1000;
}
