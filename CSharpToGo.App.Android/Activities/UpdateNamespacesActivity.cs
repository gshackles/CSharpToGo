using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using CSharpToGo.Core.Compiler;
using CSharpToGo.Core.Messaging;
using CSharpToGo.App.Android.ListAdapters;
using CSharpToGo.Core.Messaging.Messages;
using CSharpToGo.App.Android.Constants;

namespace CSharpToGo.App.Android.Activities
{
    [Activity(Label = "@string/UpdateNamespacesActivityLabel")]
    public class UpdateNamespacesActivity : Activity
    {
        private ListView _namespacesList;
        private NamespaceOptionsAdapter _adapter;
        private ISharedPreferences _preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UpdateNamespaces);

            _namespacesList = FindViewById<ListView>(Resource.Id.NamespacesList);

            string[] namespaces = Resources.GetStringArray(Resource.Array.Namespaces);
            _adapter = new NamespaceOptionsAdapter(this, namespaces);
            _adapter.CurrentNamespaces = Runner.Instance.GetUsings();

            _namespacesList.Adapter = _adapter;
            _namespacesList.ItemClick += namespacesList_ItemClick;

            _preferences = PreferenceManager.GetDefaultSharedPreferences(this);
        }
        
        private void namespacesList_ItemClick(object sender, ItemEventArgs e)
        {
            var option = e.View.FindViewById<CheckedTextView>(Resource.Id.NamespaceOption);

            if (!_adapter.CurrentNamespaces.Contains(option.Text))
            {
                option.Checked = true;
                _adapter.CurrentNamespaces.Add(option.Text);

                MessageHub.Instance.Publish(new AddNamespaceMessage(this, option.Text));
            }
            else
            {
                if (_preferences.GetBoolean(PreferenceKeys.ShowRemoveNamespaceMessage, true))
                {
                    new AlertDialog.Builder(this)
                        .SetTitle(Resource.String.RemoveNamespaceMessageTitle)
                        .SetMessage(Resource.String.RemoveNamespaceMessageContent)
                        .SetPositiveButton(Resource.String.RemoveNamespaceMessageOkButton, (s, a) => { })
                        .Show();

                    // disabling this for now so the message will get shown every time
                    // may or may not decide to add this back in
                    //_preferences
                    //    .Edit()
                    //    .PutBoolean(PreferenceKeys.ShowRemoveNamespaceMessage, false)
                    //    .Commit();
                }
            }
        }
    }
}