using Microsoft.Maui.Controls;
using System.Globalization;

namespace mauiapp1
{
    public partial class DevicePopup : ContentPage
    {
        public DevicePopup(List<string> devices)
        {
            InitializeComponent();
            SetAppLanguage();
            DeviceCollectionView.ItemsSource = devices;
        }
        private void SetAppLanguage()
        {
            var deviceLanguage = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = deviceLanguage;
            Thread.CurrentThread.CurrentUICulture = deviceLanguage;
        }
        protected override void OnAppearing()
        {
            deviceselector.Text = mauiapp1.Properties.Resources1.DeviceSelector;
            cancelbutton.Text = mauiapp1.Properties.Resources1.Cancel;
        }

        private async void DeviceCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (DeviceCollectionView.SelectedItem != null)
            {
                string? selectedDevice = DeviceCollectionView.SelectedItem.ToString();
                Preferences.AppPreferences.ipaddr = selectedDevice;
                await Navigation.PopModalAsync();
            }
            DeviceCollectionView.SelectedItem = null;
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}