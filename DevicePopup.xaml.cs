using Microsoft.Maui.Controls;

namespace mauiapp1
{
    public partial class DevicePopup : ContentPage
    {
        public DevicePopup(List<string> devices)
        {
            InitializeComponent();
            DeviceCollectionView.ItemsSource = devices;
        }
        protected override void OnAppearing()
        {
            deviceselector.Text = Properties.Resources.DeviceSelection;
            cancelbutton.Text = Properties.Resources.Cancel;
        }
        private async void DeviceCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (DeviceCollectionView.SelectedItem != null)
            {
                string? selectedDevice = DeviceCollectionView.SelectedItem.ToString();
                Preferences.AppPreferences.ipaddr = selectedDevice;
                await Navigation.PopModalAsync();
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
