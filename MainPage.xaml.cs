using Camera.MAUI;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Storage;
using System.IO;
using System.Text.Json;
using static mauiapp1.Preferences;

namespace mauiapp1
{
    public partial class MainPage : ContentPage
    {
        string flaskServerIP = AppPreferences.ipaddr;
        string flaskServerPort = "5000";
        private List<CameraInfo> availableCameras = new List<CameraInfo>(); //elenca le fotocamere disponibili 
        public MainPage()
        {
            InitializeComponent();
            cameraView.CamerasLoaded += CameraView_CamerasLoaded;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                //goloso codice specifico per android
                cameraPicker.IsVisible = true;
            }
            else
            {
                cameraPicker.IsVisible = false;
            }
        }
        private async void CameraView_CamerasLoaded(object? sender, EventArgs e)
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Camera access is required to take pictures.", "OK");
                return;
            }
            availableCameras = cameraView.Cameras.ToList();
            if (availableCameras.Count > 0)
            {
                cameraPicker.ItemsSource = availableCameras.Select(c => c.Name).ToList();
                cameraPicker.SelectedIndexChanged += CameraPicker_SelectedIndexChanged;
                cameraView.Camera = availableCameras.First(); //camera di default
                await cameraView.StartCameraAsync();
            }
        }
        private async void CameraPicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cameraPicker.SelectedIndex != -1)
            {
                cameraView.Camera = availableCameras[cameraPicker.SelectedIndex];
                await cameraView.StartCameraAsync();
            }
        }
        private string? MakeReadable(string captioncontent)
        {
            using JsonDocument jsonDocument = JsonDocument.Parse(captioncontent); //il contenuto è JSON, quindi possiamo semplicemente prenderlo e effettuare il parsing (si dice "parsarlo"???)
            string? readable = jsonDocument.RootElement.GetProperty("<CAPTION>").GetString(); //potrei usare anche toString, in teoria. Ma siccome GetString è proprio di JsonDocument, ho preferito usare lui.
            if(readable != null)
            {
                return readable;
            }
            else
            {
                //qua hai passato una caption nulla, non so quanto andrai avanti col codice
                return null;

            }
        }
        private async void DeleteCaption(string captionURL)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync(captionURL);
            }
        }
        private async void UploadPhoto(string uploadURL,string filePath,string fileName)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    MultipartFormDataContent content = new();
                    var fileBytes = File.ReadAllBytes(filePath); //legge tutti i bytes dell'immagine
                    content.Add(new ByteArrayContent(fileBytes), "file", fileName);
                    HttpResponseMessage response = await client.PostAsync(uploadURL, content);
                    string imageresult = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Upload Status", imageresult, "OK");
                }
                catch (Exception ex)
                {
                    string exception = ex.ToString();
                    await DisplayAlert("Uploading Error!", exception, "OK.");
                }
            }
        }
        public bool ValidateIP(string ipString) //per controllare se l'ip che "preferences" ci ha dato è un IP reale.
        {
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }
            byte tempForParsing;
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
        private async void DownloadPhoto(string downloadURL)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //questo è un modo assolutamente poco carino di fare quello che voglio fare, ma se funziona funziona. se qualcuno ha idee migliori, ditemi.
                    int attempts = 0;
                    int maximumAttempts = 30;
                    int delay = 5000; //ritardo tra un tentativo di connessione e l'altro, in millisecondi
                    while (attempts < maximumAttempts)
                    {
                        HttpResponseMessage caption = await client.GetAsync(downloadURL);
                        if (caption.IsSuccessStatusCode)
                        {
                            string captioncontent = await caption.Content.ReadAsStringAsync();
                            string? readablecaptioncontent = MakeReadable(captioncontent);
                            await DisplayAlert("Image Caption", readablecaptioncontent ?? "Your caption wasn't found.", "OK");
                            return;
                        }
                        attempts++;
                        await Task.Delay(delay);
                    }
                    await DisplayAlert("Error", "The image caption could was not found between the specified time range.", "OK");
                }
                catch (Exception ex)
                {
                    string exception = ex.ToString();
                    await DisplayAlert("Download Error!", exception, "OK.");
                }
            }
        }
        private async void TakePhoto(object sender, EventArgs e)
        {
            var stream = await cameraView.TakePhotoAsync();
            if (stream != null)
            {
                string? flaskServerIP = AppPreferences.ipaddr;
                if (!string.IsNullOrWhiteSpace(flaskServerIP) && ValidateIP(flaskServerIP))
                {
                    var result = ImageSource.FromStream(() => stream);
                    string fileName = "photo.jpg";
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                    using (var fileStream = File.Create(filePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            string uploadURL = $"http://{flaskServerIP}:{flaskServerPort}/upload";
                            UploadPhoto(uploadURL, filePath, fileName);
                            string downloadURL = $"http://{flaskServerIP}:{flaskServerPort}/download/caption.txt";
                            DownloadPhoto(downloadURL);
                            string deleteURL = $"http://{flaskServerIP}:{flaskServerPort}/delete/caption";
                            DeleteCaption(deleteURL);
                        }
                        catch (Exception ex)
                        {
                            string exception = ex.ToString();
                            await DisplayAlert("Error!", exception, "OK.");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("IP", "Your IP is null, or invalid. Please assign a real IP to the program via the Preferences tab.", "OK");
                }
            }
        }

    }
}