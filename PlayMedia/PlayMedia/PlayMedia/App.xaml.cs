using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PlayMedia
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new HomePage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //AppCenter.Start("android=3ec6fae8-6df9-48f3-a69c-70da3c5d5bf0;" + "uwp={Your UWP App secret here};" + "ios={Your iOS App secret here}", typeof(Analytics), typeof(Crashes));
            AppCenter.Start("c70c7959-8214-46d8-9bc8-bb6a099250be", typeof(Analytics), typeof(Crashes), typeof(Push));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
