using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mauiapp1
{
    internal static class PortScanNative
    {
        [DllImport("portscanner", EntryPoint = "scan_port_range")]
        internal static extern int ScanPortRange(string subnetPrefix, StringBuilder outputBuffer, int maxOutputLen);
    }
}
