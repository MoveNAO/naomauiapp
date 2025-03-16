using Microsoft.Extensions.Logging;
using Camera.MAUI;
using Controls.UserDialogs.Maui;

namespace mauiapp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseUserDialogs(() =>
                { 
                    AlertConfig.DefaultBackgroundColor = Colors.Purple;
#if ANDROID
                    AlertConfig.DefaultMessageFontFamily = "OpenSans-Regular.ttf";
#else
    AlertConfig.DefaultMessageFontFamily = "OpenSans-Regular";
#endif

                    ToastConfig.DefaultCornerRadius = 15;
                })
                .UseMauiCameraView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
