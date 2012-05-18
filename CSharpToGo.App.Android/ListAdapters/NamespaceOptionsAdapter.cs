using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;

namespace CSharpToGo.App.Android.ListAdapters
{
    public class NamespaceOptionsAdapter : BaseAdapter
    {
        private Activity _context;
        private IList<string> _namespaces;

        public IList<string> CurrentNamespaces { get; set; }

        public NamespaceOptionsAdapter(Activity context, IList<string> namespaces)
        {
            _context = context;
            _namespaces = namespaces;

            CurrentNamespaces = new List<string>();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // this is pretty inefficient but it works for now, creates a new view every time
            var view = _context.LayoutInflater.Inflate(Resource.Layout.NamespaceOption, parent, false) as LinearLayout;
            var currentNamespace = _namespaces.ElementAt(position);

            CheckedTextView option = view.FindViewById<CheckedTextView>(Resource.Id.NamespaceOption);

            option.Text = currentNamespace;
            option.Checked = CurrentNamespaces.Contains(currentNamespace);

            return view;
        }

        public override int Count
        {
            get { return _namespaces.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
    }
}