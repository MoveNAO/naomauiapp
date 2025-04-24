using Controls.UserDialogs.Maui;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace mauiapp1;

public partial class Preferences : ContentPage
{
    string? ipaddr;
    public Preferences()
    {
        InitializeComponent();
        SetAppLanguage();
    }
    public static class AppPreferences
    {
        public static string? ipaddr { get; set; }
    }
    private void SetAppLanguage()
    {
        var deviceLanguage = CultureInfo.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = deviceLanguage;
        Thread.CurrentThread.CurrentUICulture = deviceLanguage;
    }
    protected override void OnAppearing()
    {
        insertIP.Text = mauiapp1.Properties.Resources2.InsertYourIP;
        setButton.Text = mauiapp1.Properties.Resources2.SetButton;
        findIPs.Text = mauiapp1.Properties.Resources2.FindIPs;
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

    private async Task<List<string>> ScanDevices(string subnet)
    {
        List<string> foundIps = new List<string>();

        var processInfo = new ProcessStartInfo
        {
            FileName = "portscanner.exe",
            Arguments = subnet,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processInfo })
        {
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            foundIps = output
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Contains('.'))
                .ToList();
        }

        return foundIps;
    }

    private async Task<string> ScanSingleIpAsync(string ip, int port, CancellationToken cancellationToken)
    {
        try
        {
            using (var tcpClient = new TcpClient())
            {
                var connectTask = tcpClient.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(200, cancellationToken);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask && tcpClient.Connected)
                {
                    return ip;
                }
            }
        }
        catch
        {
            
        }

        return string.Empty;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        string? yourIP = GetCurrentIP();
        if (yourIP != null)
        {
            string? subnetMask = GetSubnetMask(yourIP);
            if (subnetMask != null)
            {
                string yourSubnet = GetCurrentSubnet(yourIP, subnetMask);
                string yourSubnetNoZero = GetSubnetWithoutZero(yourSubnet);
                if(DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await Task.Run(async () =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoadingOverlay.IsVisible = true;
                        });
                        List<string> foundIps = new List<string>();

                        string tempPath = Path.Combine(Path.GetTempPath(), "portscanner.exe");

                        try
                        {
                            try
                            {
                                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                                string resourceName = "mauiapp1.cportscanner.windows.portscanner.exe";

                                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                                using (FileStream fileStream = new FileStream(tempPath, FileMode.Create))
                                {
                                    if (resourceStream == null)
                                    {
                                        Console.WriteLine($"Resource not found: {resourceName}");
                                        Console.WriteLine("Available resources:");
                                        foreach (var res in assembly.GetManifestResourceNames())
                                        {
                                            Console.WriteLine($" - {res}");
                                        }
                                        return;
                                    }
                                    resourceStream.CopyTo(fileStream);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error extracting resource: {ex.Message}");
                                return;
                            }

                            var processInfo = new ProcessStartInfo
                            {
                                FileName = tempPath,
                                Arguments = yourSubnetNoZero,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (var process = new Process { StartInfo = processInfo })
                            {
                                process.Start();

                                string output = await process.StandardOutput.ReadToEndAsync();
                                process.WaitForExit();

                                foundIps = output
                                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(line => line.Contains('.'))
                                    .ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error running scanner: {ex.Message}");
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                DisplayAlert("ciao", ex.Message as string, "Ok");
                            });
                        }

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoadingOverlay.IsVisible = false;

                            var popup = new DevicePopup(foundIps);
                            if (Application.Current != null)
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
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    try
                    {
                        var cts = new CancellationTokenSource();
                        cts.CancelAfter(TimeSpan.FromSeconds(30));

                        await Task.Run(async () =>
                        {
                            MainThread.BeginInvokeOnMainThread(() => LoadingOverlay.IsVisible = true);
                            string subnet = yourSubnetNoZero;

                            var foundIps = new ConcurrentBag<string>();

                            int batchSize = 20;
                            var ipRanges = Enumerable.Range(1, 254)
                                .Select(i => $"{subnet}.{i}")
                                .Chunk(batchSize);

                            foreach (var ipBatch in ipRanges)
                            {
                                var scanTasks = ipBatch.Select(ip => ScanSingleIpAsync(ip, 5000, cts.Token));
                                var results = await Task.WhenAll(scanTasks);

                                foreach (var ip in results.Where(ip => !string.IsNullOrEmpty(ip)))
                                {
                                    foundIps.Add(ip);
                                }
                            }

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                LoadingOverlay.IsVisible = false;
                                var popup = new DevicePopup(foundIps.ToList());
                                Application.Current?.MainPage?.Navigation.PushModalAsync(popup);
                            });
                        }, cts.Token);
                    }
                    catch (Exception ex)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoadingOverlay.IsVisible = false;
                            DisplayAlert("Error", ex.Message, "OK");
                        });
                    }
                }
            }
            else
            {
                await DisplayAlert("Error", "Unable to retrieve subnet mask.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Unable to retrieve your IP address.", "OK");
        }
    }
} 