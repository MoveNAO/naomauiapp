using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace mauiapp1;

public partial class Preferences : ContentPage
{
    string ipaddr;
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
            DisplayAlert("IP Set.", "Your IP Address was succesfully set.", "OK");
        }
        else if(ipaddr == null)
        {
            DisplayAlert("Error.", "Your IP is null. Please set one.", "OK");
        }
    }
    public class PortScanner
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(60); //numero di connessioni contemporanee. stai attento.
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
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // Display alert for found device
                        page.DisplayAlert("Found", $"Device {ip} has port {port} open.", "OK");
                    });
                }
                catch
                {

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
            await Task.WhenAll(scanTasks); //carino lui. aspetta che tutto abbia finito... ognuno ha i suoi tempi. ed una persona matura li rispetta.
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        PortScanner portScanner = new PortScanner();
        await portScanner.ScanNetwork("192.168.219", this);
        List<string> foundIps = portScanner.RetrieveFoundIPs(); 
        var popup = new DevicePopup(foundIps);
        await Navigation.PushModalAsync(popup);
    }
}