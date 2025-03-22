namespace mauiapp1;

using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text.Json;

public partial class Home : ContentPage
{
    private const string FileName = "bicchieri.json";
    private Dictionary<string, bool> filledGlasses = new();
	public Home()
	{
		InitializeComponent();
        LoadGlassStatus();
	}
    //adesso proviamo a fare quella roba là dei bicchieri...
    public async void OnGlassTapped(object sender, TappedEventArgs e)
	{
        //codice per bicchieri
        if (sender is Image image && e.Parameter is string glassId)
        {
            if(image != null && image.Source != null)
            {
                string? sourceString = image.Source.ToString(); //mi dava rogne altrimenti
                bool isFull = sourceString?.Contains("bicchierepieno.png") ?? false;
                filledGlasses[glassId] = !isFull;
                image.Source = !isFull ? "bicchierepieno.png" : "bicchierevuoto.png";
                await SaveGlassStatus();
            }
        }
    }
    private async Task SaveGlassStatus()
    {
        try
        {
            string json = JsonSerializer.Serialize(filledGlasses, new JsonSerializerOptions { WriteIndented = true });
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
                filledGlasses = JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new Dictionary<string, bool>();
                foreach (var glass in filledGlasses)
                {
                    string glassName = "Glass" + glass.Key;
                    Image? image = FindByName(glassName) as Image; 
                    if (image != null)
                    {
                        image.Source = glass.Value ? "bicchierepieno.png" : "bicchierevuoto.png";
                    }
                }
            }
        }
        catch (Exception)
        {
            
        }
    }
}