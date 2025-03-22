using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
namespace mauiapp1;

public partial class Recents : ContentPage
{
    public class ScannedObject
    {
        public DateTimeOffset scandate { get; set; }
        public string? analysedcaption { get; set; }
    }
    public Recents()
	{
		InitializeComponent();
        LoadItems();
	}

    private async void LoadItems() {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        List<ScannedObject>? recentItems = await GetScannedObjectsAsync(); //dai non rompere
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (recentItems != null && recentItems.Any())
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
        string tempPath = System.IO.Path.GetTempPath();
        string filepath = Path.Combine(tempPath + "scannedobjects.json");
        try
        {
            using var stream = new FileStream(filepath, FileMode.Open);
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