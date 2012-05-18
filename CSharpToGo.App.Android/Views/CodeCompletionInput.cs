using System.Linq;
using System.Threading;
using Android.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using CSharpToGo.Core.Compiler;
using CSharpToGo.Core.Messaging;
using CSharpToGo.Core.Messaging.Messages;

namespace CSharpToGo.App.Android.Views
{
    public sealed class CodeCompletionInput : AutoCompleteTextView
    {
        private Context _context;
        private string _lastCompletionPrefix;
        private TextWatcher _textWatcher;
        private Thread _completionsThread;
        private Keycode? _lastKeyPressed;

        public CodeCompletionInput(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            _context = context;
            Threshold = 0;

            _lastKeyPressed = null;
            _textWatcher = new TextWatcher();
            _textWatcher.TextChanged += onTextChanged;
            AddTextChangedListener(_textWatcher);
            KeyPress += onKeyPress;

            SetSingleLine(false);
        }

        private void onKeyPress(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            if (e.E.Action != KeyEventActions.Down) return;

            if (e.E.KeyCode == Keycode.Enter)
            {
                if (IsPopupShowing && ListSelection != AdapterView.InvalidPosition)
                {
                    PerformCompletion();
                    DismissDropDown();

                    e.Handled = true;

                    return;
                }

                if (!CSharpToGoApplication.Options.MultiLineEditing
                    || (CSharpToGoApplication.Options.DoubleEnterToExecute && _lastKeyPressed == Keycode.Enter))
                {
                    MessageHub.Instance.Publish(new ExecuteCodeMessage(this, Text));

                    _lastKeyPressed = null;
                    e.Handled = true;
                }
            }

            _lastKeyPressed = e.E.KeyCode;
        }

        private void onTextChanged(string text)
        {
            if (text != _lastCompletionPrefix && text.EndsWith("."))
            {
                if (_completionsThread != null && _completionsThread.IsAlive)
                    _completionsThread.Abort();

                _completionsThread =
                    new Thread(() =>
                    {
                        var completions = Runner.Instance.GetCodeCompletions(text);

                        if (completions != null && completions.Count > 0)
                        {
                            Handler.Post(() =>
                            {
                                Adapter = new ArrayAdapter(_context,
                                                           Resource.Layout.CodeCompletionItem,
                                                           Resource.Id.Completion,
                                                           completions.ToList());
                                SetText(Text);
                            });
                        }
                    });

                _completionsThread.Start();
                _lastCompletionPrefix = text;
            }
        }

        protected override void PerformFiltering(Java.Lang.ICharSequence text, int keyCode)
        {
            string textToFilter = text.ToString();

            if (!string.IsNullOrEmpty(textToFilter) && textToFilter.StartsWith(_lastCompletionPrefix))
            {
                textToFilter = textToFilter.Substring(_lastCompletionPrefix.Length);

                base.PerformFiltering(new Java.Lang.String(textToFilter), keyCode);
            }
            else
            {
                DismissDropDown();
            }
        }

        protected override void ReplaceText(Java.Lang.ICharSequence text)
        {
            SetText(_lastCompletionPrefix + text);
        }

        public void SetText(string text) 
        {
            Text = text;
            SetSelection(Text.Length);
        }

        private class TextWatcher : Java.Lang.Object, ITextWatcher
        {
            public delegate void TextChangedHandler(string text);
            
            public event TextChangedHandler TextChanged;

            public void AfterTextChanged(IEditable s) { }

            public void BeforeTextChanged(Java.Lang.ICharSequence s, int start, int count, int after) { }

            public void OnTextChanged(Java.Lang.ICharSequence s, int start, int before, int count)
            {
                TextChanged(s.ToString());
            }
        }
    }
}