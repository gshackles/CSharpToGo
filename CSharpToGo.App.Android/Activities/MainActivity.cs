using System.Collections.Generic;
using System.Linq;
using Android.Text.Method;
using AndroidCore = Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Text.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CSharpToGo.Core.Compiler;
using CSharpToGo.Core.Messaging;
using CSharpToGo.Core.Messaging.Messages;
using CSharpToGo.App.Android.ListAdapters;
using CSharpToGo.App.Android.Views;

namespace CSharpToGo.App.Android.Activities
{
    [Activity(MainLauncher = true, Label = "@string/ApplicationName", Icon = "@drawable/icon", 
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class MainActivity : Activity
    {
        private ImageButton _run;
        private CodeCompletionInput _input;
        private ListView _resultList;
        private List<ExecutionResult> _results;
        private ExecutionResultAdapter _adapter;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            _results = new List<ExecutionResult>();
            _adapter = new ExecutionResultAdapter(this, _results);
            _resultList = FindViewById<ListView>(Resource.Id.Results);
            _resultList.Adapter = _adapter;
            
            _run = FindViewById<ImageButton>(Resource.Id.Run);
            _input = FindViewById<CodeCompletionInput>(Resource.Id.Input);

            _run.Click += delegate { runCode(_input.Text); };

            MessageHub.Instance.Subscribe<ResultInputSelectedMessage>(msg => _input.SetText(msg.Content));
            MessageHub.Instance.Subscribe<ExecuteCodeMessage>(msg => runCode(msg.Content));
            MessageHub.Instance.Subscribe<MessageLongClickedMessage>(msg => onMessageLongClicked(msg.Content));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);

            new MenuInflater(this).Inflate(Resource.Menu.OptionsMenu, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.Clear:
                    var progressDialog = ProgressDialog.Show(this,
                                                             Resources.GetString(
                                                                 Resource.String.ClearSessionProgressTitle),
                                                             Resources.GetString(Resource.String.ClearSessionProgressText),
                                                             true);

                    Runner.Instance.ClearSession(() => RunOnUiThread(() =>
                    {
                        _results.Clear();
                        _adapter.NotifyDataSetChanged();
                        progressDialog.Hide();
                    }));

                    return true;
                case Resource.Id.Namespaces:
                    StartActivity(typeof(UpdateNamespacesActivity));

                    return true;
                case Resource.Id.Variables:
                    showVariables();

                    return true;
                case Resource.Id.Settings:
                    StartActivity(typeof(SettingsActivity));

                    return true;
                case Resource.Id.About:
                    var packageInfo = PackageManager.GetPackageInfo(PackageName, 0);
                    var aboutContent =
                        new SpannableString(string.Format(Resources.GetString(Resource.String.AboutContent),
                                                          packageInfo.VersionName));

                    Linkify.AddLinks(aboutContent, MatchOptions.All);

                    var dialog = 
                        new AlertDialog.Builder(this)
                            .SetTitle(Resource.String.AboutTitle)
                            .SetMessage(aboutContent)
                            .SetPositiveButton(Resource.String.AboutOkButton, delegate { })
                            .Show();

                    
                    ((TextView)dialog.FindViewById(AndroidCore.Resource.Id.Message)).MovementMethod = LinkMovementMethod.Instance;

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void runCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var progressDialog = ProgressDialog.Show(this, 
                                                     Resources.GetString(Resource.String.RunningCodeProgressTitle), 
                                                     Resources.GetString(Resource.String.RunningCodeProgressText), 
                                                     true);

            Runner.Instance.RunAsync(input, result => RunOnUiThread(() =>
            {
                _results.Add(result);
                _adapter.NotifyDataSetChanged();
                _input.Text = "";
                progressDialog.Hide();

                ((InputMethodManager)GetSystemService(InputMethodService))
                    .HideSoftInputFromWindow(_input.WindowToken, HideSoftInputFlags.ImplicitOnly);
            }));
        }

        private void showVariables()
        {
            var variables = Runner.Instance.GetVariables()
                                .OrderBy(variable => variable.Name);

            if (variables.Count() == 0)
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.NoVariables), ToastLength.Short).Show();
            }
            else
            {
                var namesToDisplay = variables
                                        .Select(variable => variable.Name + " : " + variable.TypeName)
                                        .ToArray();

                new AlertDialog.Builder(this)
                    .SetTitle(Resources.GetString(Resource.String.VariablesDialogTitle))
                    .SetCancelable(true)
                    .SetItems(namesToDisplay, (sender, args) =>
                    {
                        var variableName = variables.ElementAt((int)args.Which).Name;

                        _input.SetText(_input.Text + variableName);
                    })
                    .Show();
            }
        }

        private void onMessageLongClicked(string message)
        {
            var menuItems = new string[] 
                {
                    Resources.GetString(Resource.String.CopyMessage),
                    Resources.GetString(Resource.String.SearchForMessage)
                };

            new AlertDialog.Builder(this)
                    .SetCancelable(true)
                    .SetItems(menuItems, (sender, args) =>
                    {
                        int which = (int)args.Which;

                        if (which == 0)
                        {
                            var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                            clipboard.Text = message;
                        }
                        else
                        {
                            var searchIntent = new Intent(Intent.ActionWebSearch);
                            searchIntent.PutExtra(SearchManager.Query, message);
                            StartActivity(searchIntent);
                        }
                    })
                    .Show();
        }
    }
}

