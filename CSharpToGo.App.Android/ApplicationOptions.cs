namespace CSharpToGo.App.Android
{
    public class ApplicationOptions
    {
        public bool MultiLineEditing { get; set; }
        public bool DoubleEnterToExecute { get; set; }

        public ApplicationOptions()
        {
            MultiLineEditing = true;
            DoubleEnterToExecute = false;
        }
    }
}