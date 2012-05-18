using System;
using System.Collections.Generic;
using System.Linq;
using CSharpToGo.Core.Compiler;
using Android.App;
using Android.Content;
using Android.Preferences;
using CSharpToGo.Core.Messaging;
using CSharpToGo.App.Android.Constants;
using CSharpToGo.Core.Messaging.Messages;

namespace CSharpToGo.App.Android
{
    [Application(Theme = "@style/ApplicationTheme", Label = "@string/ApplicationName", Icon = "@drawable/icon")]
    public class CSharpToGoApplication : Application
    {
        private SharedPreferenceChangeListener _preferenceChangeListener;

        public CSharpToGoApplication(IntPtr handle)
            : base(handle)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            
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

        private class SharedPreferenceChangeListener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
        {
            private readonly ISharedPreferences _preferences;

            public SharedPreferenceChangeListener(Context context)
            {
                _preferences = PreferenceManager.GetDefaultSharedPreferences(context);

                MessageHub.Instance.Subscribe<AddNamespaceMessage>(msg => addNamespace(msg.Content));
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                switch (key)
                {
                    case PreferenceKeys.Timeout:
                        Runner.Instance.Options.Timeout = sharedPreferences.GetInt(key, Runner.Instance.Options.Timeout);
                        break;
                    case PreferenceKeys.SaveNamespaces:
                        if (!sharedPreferences.GetBoolean(key, false))
                            resetSavedNamespaces();

                        break;
                }
            }

            private void addNamespace(string @namespace)
            {
                var currentNamespaces = _preferences
                                            .GetString(PreferenceKeys.SavedNamespaces, "")
                                            .Split(',')
                                            .ToList();
                currentNamespaces.Add(@namespace);

                _preferences
                    .Edit()
                    .PutString(PreferenceKeys.SavedNamespaces, string.Join(",", currentNamespaces.Distinct()))
                    .Commit();

                if (_preferences.GetBoolean(PreferenceKeys.SaveNamespaces, false))
                    Runner.Instance.Options.DefaultNamespaces = new List<string>(currentNamespaces);
            }

            private void resetSavedNamespaces()
            {
                Runner.Instance.Options.ResetDefaultNamespaces();

                _preferences
                    .Edit()
                    .Remove(PreferenceKeys.SavedNamespaces)
                    .Commit();

                foreach (var @namespace in Runner.Instance.Options.DefaultNamespaces)
                    addNamespace(@namespace);
            }
        }
    }
}