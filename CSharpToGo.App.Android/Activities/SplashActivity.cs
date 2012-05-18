using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace CSharpToGo.App.Android.Activities
{
    [Activity(MainLauncher = true, Label = "@string/ApplicationName", Icon = "@drawable/icon",
              Theme = "@style/Theme.Splash", NoHistory = true,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StartActivity(typeof(MainActivity));
        }
    }
}