using System.Collections.Generic;

namespace CSharpToGo.Core.Compiler
{
    public class ExecutionResult
    {
        public string Input { get; set; }
        public string Output { get; set; 
        }
        public string ConsoleOutput { get; set; }

        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
    }
}