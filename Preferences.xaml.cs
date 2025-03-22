using Controls.UserDialogs.Maui;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace mauiapp1;

public partial class Preferences : ContentPage
{
    string? ipaddr;
    public Preferences()
    {
        InitializeComponent();
    }
    public static class AppPreferences
    {
        public static string? ipaddr { get; set; }
    }
    public void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ipaddr = e.NewTextValue;
    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        if (ipaddr != null)
        {
            AppPreferences.ipaddr = ipaddr;
            DisplayAlert(Properties.Resources.SetIP, Properties.Resources.IpSuccess, Properties.Resources.OK);
        }
        else if (ipaddr == null)
        {
            DisplayAlert(Properties.Resources.Error, Properties.Resources.IpNull, Properties.Resources.OK);
        }
    }
    public class PortScanner
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(150); //numero di connessioni contemporanee. stai attento.
        private List<string> _foundIPs = new List<string>();
        public async Task ScanPort(string ip, int port, ContentPage page)
        {
            await _semaphore.WaitAsync();
            try
            {
                using TcpClient client = new TcpClient();
                try
                {
                    await client.ConnectAsync(ip, port);
                    _foundIPs.Add(ip);
                }
                catch
                {
                    //cos� il codice va avanti e non si blocca quando ha trovato un errore. qualcuno mi dica che non si fa cos�, vi prego.
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public List<string> RetrieveFoundIPs() => _foundIPs; //ma "retreive" o "retrieve" come si scrive?

        public async Task ScanNetwork(string subnet, ContentPage page)
        {
            List<Task> scanTasks = new List<Task>();

            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}.{i}";
                scanTasks.Add(ScanPort(ip, 5000, page));
            }
            await Task.WhenAll(scanTasks); //carino lui. aspetta che tutto abbia finito... 
        }
    }

    private static string? GetCurrentIP()
    {
        string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            try
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    localIP = endPoint.Address.ToString();
                    return localIP;
                }
                return null;
            }
            catch
            {
                return null; //sei offline
            }
        }
    }

    private static string? GetSubnetMask(string ipAddress)
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString() == ipAddress)
                {
                    return ip.IPv4Mask.ToString(); //ho bisogno di una stringa
                }
            }
        }
        return null;
    }

    private static string GetCurrentSubnet(string yourIP, string subnetMask)
    {
        byte[] ipBytes = IPAddress.Parse(yourIP).GetAddressBytes();
        byte[] maskBytes = IPAddress.Parse(subnetMask).GetAddressBytes();
        byte[] subnetBytes = new byte[ipBytes.Length];

        for (int i = 0; i < ipBytes.Length; i++)
        {
            subnetBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
        }

        return new IPAddress(subnetBytes).ToString();
    }

    private static string GetSubnetWithoutZero(string subnet)
    {
        return subnet.EndsWith(".0") ? subnet.Substring(0, subnet.LastIndexOf(".")) : subnet;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        PortScanner portScanner = new PortScanner();
        string? yourIP = GetCurrentIP();
        if (yourIP != null)
        {
            string? subnetMask = GetSubnetMask(yourIP);
            if (subnetMask != null)
            {
                string yourSubnet = GetCurrentSubnet(yourIP, subnetMask);
                string yourSubnetnozero = GetSubnetWithoutZero(yourSubnet);
                await Task.Run(async () =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoadingOverlay.IsVisible = true;
                    });

                    await portScanner.ScanNetwork(yourSubnetnozero, this);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoadingOverlay.IsVisible = false;
                    });
                    List<string> foundIps = portScanner.RetrieveFoundIPs();
                    var popup = new DevicePopup(foundIps);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if(Application.Current != null) //compilatore come cavolo fai a pensare che questo possa essere nullo? "dereference of a possibly null reference", ma se l'applicazione corrente è nulla cosa diamine staresti facendo con il tuo computer?
                        {
                            Window? window = Application.Current.Windows.Count > 0 ? Application.Current.Windows[0] : null;
                            if (window != null && window.Page != null)
                            {
                                window.Page.Navigation.PushModalAsync(popup);
                            }
                        }
                    });
                });
            }
            else
            {
                await DisplayAlert(Properties.Resources.Error, Properties.Resources.Offline, Properties.Resources.OK);
            }
        }
    } }