using Android.App;
using Android.OS;
using Android.Preferences;

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
    }
}