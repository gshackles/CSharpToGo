using Android.App;
using Android.OS;
using Android.Preferences;
using CSharpToGo.Core.Compiler;

namespace CSharpToGo.App.Android.Activities
{
    [Activity(Label = "@string/SettingsLabel", Theme = "@style/PreferencesTheme")]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            AddPreferencesFromResource(Resource.Xml.Settings);
        }

        protected override void OnPause()
        {
            base.OnPause();

            var preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            Runner.Instance.Options.Timeout = preferences.GetInt("timeout", Runner.Instance.Options.Timeout);
        }
    }
}