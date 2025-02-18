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
}