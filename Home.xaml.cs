namespace mauiapp1;

using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text.Json;

public partial class Home : ContentPage
{
    private const string FileName = "bicchieri.json";
    private Dictionary<string, bool> _filledGlasses = new();
	public Home()
	{
		InitializeComponent();
        LoadGlassStatus();
	}
    //adesso proviamo a fare quella roba là dei bicchieri...
    private async void OnGlassTapped(object sender, TappedEventArgs e)
    {
        try
        {
            //codice per bicchieri
            if (sender is not Image image || e.Parameter is not string glassId) return;
            if (image.Source == null) return;
            string? sourceString = image.Source.ToString(); //mi dava rogne altrimenti
            bool isFull = sourceString?.Contains("bicchierepieno.png") ?? false;
            _filledGlasses[glassId] = !isFull;
            image.Source = !isFull ? "bicchierepieno.png" : "bicchierevuoto.png";
            await SaveGlassStatus();
        }
        catch (Exception er)
        {
            throw;
        }
    }
    private async Task SaveGlassStatus()
    {
        try
        {
            string json = JsonSerializer.Serialize(_filledGlasses, new JsonSerializerOptions { WriteIndented = true });
            string filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception)
        {
            
        }
    }
    private async void LoadGlassStatus()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
            //Process.Start("notepad.exe", filePath);
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                _filledGlasses = JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new Dictionary<string, bool>();
                foreach (var glass in _filledGlasses)
                {
                    var glassName = "Glass" + glass.Key;
                    if (FindByName(glassName) is Image image)
                    {
                        image.Source = glass.Value ? "bicchierepieno.png" : "bicchierevuoto.png";
                    }
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}