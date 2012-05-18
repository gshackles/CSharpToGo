using Android.Views;

namespace CSharpToGo.App.Android.Extensions
{
    public static class ViewExtensions
    {
        public static View ShowIf(this View view, bool show)
        {
            view.Visibility = show
                                ? ViewStates.Visible
                                : ViewStates.Gone;

            return view;
        }
    }
}