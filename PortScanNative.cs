using System.Runtime.InteropServices;
using System.Text;
namespace mauiapp1;

internal static class PortScanNative
{
    [DllImport("portscanner", EntryPoint = "scan_port_range", CharSet = CharSet.Unicode)]
    internal static extern int ScanPortRange(string subnetPrefix, StringBuilder outputBuffer, int maxOutputLen);
}