using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Android.App;
using Android.Text;
using Android.Text.Style;
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
        private const string _keywordRegex = @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while)\b";

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

            setTextWithSyntaxHighlighting(input, result.Input);

            if (convertView == null)
            {
                input.Click += (sender, args) =>
                               MessageHub.Instance.Publish(new ResultInputSelectedMessage(input, input.Text));
            }

            output.Text = result.Output;
            output.ShowIf(result.Errors.Count == 0);

            consoleOutput.Text = result.ConsoleOutput;
            consoleOutputContainer.ShowIf(!string.IsNullOrEmpty(result.ConsoleOutput));

            addMessages(view, Resource.Id.Errors, result.Errors, Resource.Layout.ErrorMessageItem);
            
            return view;
        }

        private void setTextWithSyntaxHighlighting(TextView view, string input)
        {
            // don't display a trailing semicolon if there is one
            if (input.EndsWith(";"))
            {
                input = input.Substring(0, input.Length - 1);
            }

            var builder = new SpannableStringBuilder(input);
            var highlightColor = _context.Resources.GetColor(Resource.Color.SyntaxHighlight);

            var keywordMatches = Regex.Matches(input, _keywordRegex);

            foreach (Match match in keywordMatches) 
            {
                var highlightColorSpan = new ForegroundColorSpan(highlightColor);
                builder.SetSpan(highlightColorSpan, match.Index, match.Index + match.Length, SpanTypes.InclusiveInclusive);
            }

            view.TextFormatted = builder;
        }

        private void addMessages(LinearLayout view, int containerResourceId, IEnumerable<string> messages, int itemResourceId)
        {
            LinearLayout container = view.FindViewById<LinearLayout>(containerResourceId);

            container.RemoveAllViews();

            foreach (string message in messages)
            {
                var messageView = _context.LayoutInflater.Inflate(itemResourceId, null);
                messageView.FindViewById<TextView>(Resource.Id.MessageText).Text = message;

                messageView.LongClick += (sender, args) =>
                {
                    MessageHub.Instance.Publish(new MessageLongClickedMessage(messageView, message));
                };
                container.AddView(messageView);
            }
        }
    }
}