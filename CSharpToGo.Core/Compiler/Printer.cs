using System;
using Mono.CSharp;

namespace CSharpToGo.Core.Compiler
{
    public class Printer : ReportPrinter
    {
        private Action<AbstractMessage> _errorHandler, _warningHandler;

	    public Printer(Action<AbstractMessage> errorHandler, Action<AbstractMessage> warningHandler)
	    {
		    _errorHandler = errorHandler;
		    _warningHandler = warningHandler;
	    }

	    public override void Print (AbstractMessage msg) {
		    base.Print(msg);
		
		    if (msg.MessageType == "error")
			    _errorHandler(msg);
		    else if (msg.MessageType == "warning")
			    _warningHandler(msg);
	    }
    }
}