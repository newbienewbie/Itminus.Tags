using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Itminus.Tags.ZLan
{
    public class ZLanScanOptions
    {
        public string DevName { get; set; } = "ZLAN-001";
        public byte SlaveAddr { get; } = 1;
        public string IpAddr { get; set; } = "localhost";
        public int Port { get; set; }
        public int ConnectionTimetout { get; set; } = 1000;
        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;
        public int ScanInterval { get; set; } = 200;
        public int ErrorInterval { get; set; } = 500;
    }
}
