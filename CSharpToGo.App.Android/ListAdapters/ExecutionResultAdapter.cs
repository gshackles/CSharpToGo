using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using CSharpToGo.Core.Compiler;
using CSharpToGo.Core.Messaging;
using CSharpToGo.Core.Messaging.Messages;
using CSharpToGo.App.Android.Extensions;

namespace CSharpToGo.App.Android.ListAdapters
{
    public class ExecutionResultAdapter : BaseAdapter
    {
        private IList<ExecutionResult> _results;
        private Activity _context;

        public ExecutionResultAdapter(Activity context, IList<ExecutionResult> results)
        {
            _context = context;
            _results = results;
        }

        public override int Count
        {
            get { return _results.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = (convertView
                            ?? _context.LayoutInflater.Inflate(
                                    Resource.Layout.ResultItem, parent, false)
                        ) as LinearLayout;
            var result = _results.ElementAt(position);
            
            TextView input = view.FindViewById<TextView>(Resource.Id.Input),
                     output = view.FindViewById<TextView>(Resource.Id.Output),
                     consoleOutput = view.FindViewById<TextView>(Resource.Id.ConsoleOutput);
            RelativeLayout consoleOutputContainer = view.FindViewById<RelativeLayout>(Resource.Id.ConsoleOutputContainer);
            
            // don't display a trailing semicolon if there is one
            input.Text = result.Input.EndsWith(";")
                            ? result.Input.Substring(0, result.Input.Length - 1)
                            : result.Input;

            input.Click += (sender, args) =>
                MessageHub.Instance.Publish(new ResultInputSelectedMessage(input, input.Text));

            output.Text = result.Output;
            output.ShowIf(result.Errors.Count == 0);

            consoleOutput.Text = result.ConsoleOutput;
            consoleOutputContainer.ShowIf(!string.IsNullOrEmpty(result.ConsoleOutput));

            addMessages(view, Resource.Id.Errors, result.Errors, Resource.Layout.ErrorMessageItem);
            
            return view;
        }

        private void addMessages(LinearLayout view, int containerResourceId, IList<string> messages, int itemResourceId)
        {
            LinearLayout container = view.FindViewById<LinearLayout>(containerResourceId);

            container.RemoveAllViews();

            foreach (string message in messages)
            {
                var messageView = _context.LayoutInflater.Inflate(itemResourceId, null);
                messageView.FindViewById<TextView>(Resource.Id.MessageText).Text = message;
                
                messageView.LongClick = onMessageLongClicked;

                container.AddView(messageView);
            }
        }

        private bool onMessageLongClicked(View view) 
        {
            string message = view.FindViewById<TextView>(Resource.Id.MessageText).Text;

            MessageHub.Instance.Publish(new MessageLongClickedMessage(view, message));

            return true;
        }
    }
}