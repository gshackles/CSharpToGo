using System;
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using CSharpToGo.Core.Compiler;
using Android.App;
using Android.Preferences;
using CSharpToGo.App.Android.Constants;

namespace CSharpToGo.App.Android
{
    [Application(Theme = "@style/ApplicationTheme", Label = "@string/ApplicationName", Icon = "@drawable/icon")]
    public class CSharpToGoApplication : Application
    {
        private SharedPreferenceChangeListener _preferenceChangeListener;

        public static ApplicationOptions Options { get; private set; }

        public CSharpToGoApplication(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
         
            Options = new ApplicationOptions();
            Runner.Instance.Options.TimeoutMessage = Resources.GetString(Resource.String.TimeoutMessage);
            Runner.Instance.Options.UsingMessage = Resources.GetString(Resource.String.UsingMessage);
            Runner.Instance.Options.ClassMessage = Resources.GetString(Resource.String.ClassMessage);

            _preferenceChangeListener = new SharedPreferenceChangeListener(this);

            var preferences = PreferenceManager.GetDefaultSharedPreferences(this);
            preferences.RegisterOnSharedPreferenceChangeListener(_preferenceChangeListener);

            if (preferences.GetBoolean(PreferenceKeys.SaveNamespaces, false))
            {
                string namespaces = preferences.GetString(PreferenceKeys.SavedNamespaces, "");

                if (!string.IsNullOrEmpty(namespaces))
                    Runner.Instance.Options.DefaultNamespaces = new List<string>(namespaces.Split(','));
            }
        }
    }
}