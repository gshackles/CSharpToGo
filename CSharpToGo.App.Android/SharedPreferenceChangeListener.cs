using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Preferences;
using CSharpToGo.App.Android.Constants;
using CSharpToGo.Core.Compiler;
using CSharpToGo.Core.Messaging;
using CSharpToGo.Core.Messaging.Messages;

namespace CSharpToGo.App.Android
{
    public class SharedPreferenceChangeListener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
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
                case PreferenceKeys.MultiLineEditing:
                    CSharpToGoApplication.Options.MultiLineEditing = sharedPreferences.GetBoolean(key, CSharpToGoApplication.Options.MultiLineEditing);

                    break;
                case PreferenceKeys.DoubleEnter:
                    CSharpToGoApplication.Options.DoubleEnterToExecute = sharedPreferences.GetBoolean(key, CSharpToGoApplication.Options.DoubleEnterToExecute);

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