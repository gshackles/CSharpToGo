using System.Collections.Generic;

namespace CSharpToGo.Core.Compiler
{
    public class RunnerOptions
    {
        private static readonly List<string> _defaultNamespacesInitialValue = 
            new List<string> {"System", "System.Linq"};

        public int Timeout { get; set; }
        public string TimeoutMessage { get; set; }
        public string UsingMessage { get; set; }
        public string ClassMessage { get; set; }
        public IList<string> DefaultNamespaces { get; set; }

        public RunnerOptions()
        {
            Timeout = 30;
            TimeoutMessage = "Timeout!";
            UsingMessage = "Using statements not allowed";
            ClassMessage = "Class declarations not allowed";
            ResetDefaultNamespaces();
        }

        public void ResetDefaultNamespaces()
        {
            DefaultNamespaces = new List<string>(_defaultNamespacesInitialValue);
        }
    }
}