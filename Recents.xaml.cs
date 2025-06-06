using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
namespace mauiapp1;

public abstract partial class Recents : ContentPage
{
    public abstract class ScannedObject
    {
        public DateTimeOffset Scandate { get; set; }
        public string? Analysedcaption { get; set; }
    }

    protected Recents()
	{
		InitializeComponent();
        LoadItems();
	}

    private static void SetAppLanguage()
    {
        var deviceLanguage = CultureInfo.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = deviceLanguage;
        Thread.CurrentThread.CurrentUICulture = deviceLanguage;
    }
    protected override void OnAppearing()
    {
        the_nothingness.Text = mauiapp1.Properties.Resources3.Nothingness;
    }

    private async void LoadItems() {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        List<ScannedObject>? recentItems = await GetScannedObjectsAsync(); //dai non rompere
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (recentItems != null && recentItems.Count != 0)
        {
            recentItemsListView.ItemsSource = recentItems;
        }
        else
        {
            //non c'è nulla nella lista dei recenti
            recentItemsListView.IsVisible = false;
            the_nothingness.IsVisible = true;
        }
    }
    public static async Task<List<ScannedObject>?>? GetScannedObjectsAsync() 
    {
        var tempPath = System.IO.Path.GetTempPath();
        var filepath = Path.Combine(tempPath + "scannedobjects.json");
        try
        {
            await using var stream = new FileStream(filepath, FileMode.Open);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<ScannedObject>>(json);
            }
        }
        catch (Exception)
        {
            //non c'è niente qui
        }
        return null;
    }
}