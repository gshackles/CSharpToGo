using Android.Content;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CSharpToGo.App.Android.Views
{
    public class SeekBarPreference : DialogPreference, SeekBar.IOnSeekBarChangeListener
    {
        private static readonly string _applicationNamespace = "http://schemas.android.com/apk/res/com.gregshackles.csharptogo";

        private SeekBar _seekBar;
        private TextView _valueText;

        private string _suffix;
        private int _minimumValue, _maximumValue, _defaultValue, _progress;

        public SeekBarPreference(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            _progress = 0;
            _minimumValue = attrs.GetAttributeIntValue(_applicationNamespace, "minValue", 0);
            _defaultValue = attrs.GetAttributeIntValue(_applicationNamespace, "defaultValue", 0) - _minimumValue;
            _maximumValue = attrs.GetAttributeIntValue(_applicationNamespace, "maxValue", 100) - _minimumValue;

            int suffixResourceId = attrs.GetAttributeResourceValue(_applicationNamespace, "suffix", 0);

            if (suffixResourceId > 0)
                _suffix = " " + context.Resources.GetString(suffixResourceId);

            DialogLayoutResource = Resource.Layout.TimeoutDialogPreference;
        }
        
        protected override void OnBindDialogView(View view)
        {
            base.OnBindDialogView(view);

            if (ShouldPersist())
                _progress = GetPersistedInt(_defaultValue + _minimumValue) - _minimumValue;


            _valueText = view.FindViewById<TextView>(Resource.Id.Label);

            _seekBar = view.FindViewById<SeekBar>(Resource.Id.SeekBar);
            _seekBar.SetOnSeekBarChangeListener(this);
            _seekBar.Max = _maximumValue;
            _seekBar.Progress = _progress;

            updateText();
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            _progress = progress;

            updateText();

            CallChangeListener(progress);
        }

        protected override void OnDialogClosed(bool positiveResult)
        {
            base.OnDialogClosed(positiveResult);

            if (positiveResult)
                PersistInt(_progress + _minimumValue);
        }

        public void OnStartTrackingTouch(SeekBar seekBar) { }

        public void OnStopTrackingTouch(SeekBar seekBar) { }

        private void updateText()
        {
            _valueText.Text = (_progress + _minimumValue).ToString();

            if (!string.IsNullOrEmpty(_suffix))
                _valueText.Text += _suffix;
        }
    }
}