using Camera.MAUI;
using System.Globalization;
using System.Text.Json;
using static mauiapp1.Preferences;

namespace mauiapp1
{
    public class ScannedObject
    {
        public DateTimeOffset scandate { get; set; }
        public string? analysedcaption { get; set; }
    }
    public partial class MainPage : ContentPage
    {
        string? flaskServerIP = AppPreferences.ipaddr;
        string flaskServerPort = "5000";
        private bool _disposed;
        public CameraInfo selectedCamera;
        private List<CameraInfo> availableCameras = new List<CameraInfo>(); //elenca le fotocamere disponibili 
        public MainPage()
        {
            InitializeComponent();
            SetAppLanguage();
            try
            {
                cameraView.CamerasLoaded += CameraView_CamerasLoaded;
            }
            catch (Exception)
            {

            }
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

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                //la pagina ha problemi di reload quando è cambiata da android. così la pagina viene ricaricata forzatamente.
                cameraPicker.Title = Properties.Resources.SetCamera;
                CounterBtn.Text = Properties.Resources.TakeAPicture;
                loadinglabel.Text = Properties.Resources.Loading;
                var temp = BindingContext;
                BindingContext = null;
                BindingContext = temp;
                cameraView.CamerasLoaded -= CameraView_CamerasLoaded; //forse non detona se prima disattiviamo
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
                await cameraView.StartCameraAsync().ConfigureAwait(true);
            }
            catch (Exception)
            {
                
            }
        }

        private void SetAppLanguage()
        {
            var deviceLanguage = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = deviceLanguage;
            Thread.CurrentThread.CurrentUICulture = deviceLanguage;
        }

        private async void CameraView_CamerasLoaded(object? sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(true);
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert(mauiapp1.Properties.Resources.PermissionDenied, 
                              Properties.Resources.CameraAccessIsRequiredToTakePictures, 
                              Properties.Resources.OK).ConfigureAwait(true);
                    return;
                }
                if (cameraView?.Cameras == null)
                {
                    await DisplayAlert(Properties.Resources.Error, 
                              "Camera initialization failed", 
                              Properties.Resources.OK).ConfigureAwait(true);
                    return;
                }
                availableCameras.Clear();
                availableCameras = cameraView.Cameras
                    .GroupBy(c => c.Name)
                    .Select(g => g.First())
                    .ToList();

                if (availableCameras.Count > 0)
                {
                    cameraPicker.ItemsSource = availableCameras.Select(c => c.Name).ToList();
                    cameraPicker.SelectedIndexChanged += CameraPicker_SelectedIndexChanged;
                    cameraView.Camera = availableCameras.First();
                    await cameraView.StartCameraAsync().ConfigureAwait(true);
                }
                else
                {
                    await DisplayAlert(Properties.Resources.Error,
                                Properties.Resources.NoCamerasFound,
                                Properties.Resources.OK).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(Properties.Resources.Error, 
                          $"Camera initialization error: {ex.Message}", 
                          Properties.Resources.OK).ConfigureAwait(true);
            }
        }

        private async void CameraPicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cameraPicker.SelectedIndex != -1)
            {
                try
                {
                    string? selectedCameraName = cameraPicker.SelectedItem.ToString();
                    string selectedCameraMessage = Properties.Resources.SelectedCamera;
                    Console.WriteLine(selectedCameraMessage + selectedCameraName);
                    CameraPosition cameraPosition = CameraPosition.Back;
                    if(selectedCameraName != null)
                    {
                        if (selectedCameraName.Equals("Rear Camera", StringComparison.OrdinalIgnoreCase))
                        {
                            cameraPosition = CameraPosition.Back;
                        }
                        var cameras = cameraView.Cameras;
                        selectedCamera = cameras.FirstOrDefault(c => c.Position == cameraPosition);
                        if (selectedCamera != null)
                        {
                            cameraView.Camera = selectedCamera;
                            await cameraView.StartCameraAsync().ConfigureAwait(true);
                        }
                    }                    
                    else
                    {
                        await DisplayAlert(Properties.Resources.Error, 
                                  Properties.Resources.CameraNotFound, 
                                  "OK").ConfigureAwait(true);
                    }
                }
                catch (Exception ex)
                {
                    string message = Properties.Resources.FailedToStartTheCamera;
                    await DisplayAlert(Properties.Resources.Error, 
                              message + ex.Message, 
                              Properties.Resources.OK).ConfigureAwait(true);
                }
            }
        }

        private string? MakeReadable(string captioncontent)
        {
            using JsonDocument jsonDocument = JsonDocument.Parse(captioncontent);
            
            if (jsonDocument.RootElement.TryGetProperty("<CAPTION>", out JsonElement captionElement))
            {
                if (captionElement.ValueKind == JsonValueKind.String)
                {
                    return captionElement.GetString();
                }
                else if (captionElement.ValueKind == JsonValueKind.Object)
                {
                    if (captionElement.TryGetProperty("text", out JsonElement textElement))
                    {
                        return textElement.GetString();
                    }
                }
            }
            return null;
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
                    await DisplayAlert(Properties.Resources.UploadStatus, 
                              imageresult, 
                              Properties.Resources.OK).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    string exception = ex.ToString();
                    await DisplayAlert(Properties.Resources.UploadingError, 
                              exception, 
                              Properties.Resources.OK).ConfigureAwait(true);
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
        private async void DownloadCaption(string downloadURL)
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
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoadingOverlay.IsVisible = true;
                        });
                        HttpResponseMessage caption = await client.GetAsync(downloadURL).ConfigureAwait(true);
                        if (caption.IsSuccessStatusCode)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                LoadingOverlay.IsVisible = false;
                            });
                            string captioncontent = await caption.Content.ReadAsStringAsync().ConfigureAwait(true);
                            string? readablecaptioncontent = MakeReadable(captioncontent);
                            await DisplayAlert(Properties.Resources.ImageCaption, 
                                      readablecaptioncontent ?? Properties.Resources.YourCaptionWasnTFound, 
                                      Properties.Resources.OK).ConfigureAwait(true);
                            if (readablecaptioncontent != null)
                            {
                                PrintToJSON(readablecaptioncontent);
                            }
                            return;
                        }
                        attempts++;
                        await Task.Delay(delay).ConfigureAwait(true);
                    }
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoadingOverlay.IsVisible = false;
                    });
                    await DisplayAlert(Properties.Resources.Error, 
                              Properties.Resources.ImgCaptionNotFound, 
                              Properties.Resources.OK).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    string exception = ex.ToString();
                    await DisplayAlert(Properties.Resources.Error, 
                              exception, 
                              Properties.Resources.OK).ConfigureAwait(true);
                }
            }
        }
        private async void TakePhoto(object sender, EventArgs e)
        {
            try
            {
                var stream = await cameraView.TakePhotoAsync().ConfigureAwait(true);
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
                            await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                        }
                        using (HttpClient client = new HttpClient())
                        {
                            try
                            {
                                string uploadURL = $"http://{flaskServerIP}:{flaskServerPort}/upload";
                                UploadPhoto(uploadURL, filePath, fileName);
                                string downloadURL = $"http://{flaskServerIP}:{flaskServerPort}/download/caption.txt";
                                DownloadCaption(downloadURL);
                                string deleteURL = $"http://{flaskServerIP}:{flaskServerPort}/delete/caption";
                                DeleteCaption(deleteURL);
                            }
                            catch (Exception ex)
                            {
                                string exception = ex.ToString();
                                await DisplayAlert(Properties.Resources.Error, exception, Properties.Resources.OK).ConfigureAwait(true);
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("IP",
                                  Properties.Resources.NullInvalidIP,
                                  Properties.Resources.OK).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void PrintToJSON(string caption)
        {
            //stampa caption e data corrente in un JSON
            DateTime thisDay = DateTime.Today;
            var scannedObject = new ScannedObject
            {
                scandate = thisDay,
                analysedcaption = caption
            };
            string tempPath = System.IO.Path.GetTempPath();
            string filename = tempPath + "\\scannedobjects.json";
            List<ScannedObject> scannedObjects = new();
            if (File.Exists(filename))
            {
                try
                {
                    string existingJSON = File.ReadAllText(filename); //così prende tutti i dati dal file esistente
                    if (!string.IsNullOrWhiteSpace(existingJSON))
                    {
                        scannedObjects = JsonSerializer.Deserialize<List<ScannedObject>>(existingJSON) ?? new List<ScannedObject>();
                    }
                }
                catch (JsonException) //oh ma esiste
                {

                }
                scannedObjects.Add(scannedObject);
                string scannedObjectJSONString = JsonSerializer.Serialize(scannedObjects, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filename, scannedObjectJSONString);
            }
        }
        protected override void OnDisappearing()
        {
            //forse così
            base.OnDisappearing();
            try
            {
                cameraView.StopCameraAsync();
                cameraView.CamerasLoaded -= CameraView_CamerasLoaded;
            }
            catch (Exception)
            {
            }
        }
    }
}   
