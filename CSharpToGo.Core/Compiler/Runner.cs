using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpToGo.Core.Messaging;
using CSharpToGo.Core.Messaging.Messages;
using Mono.CSharp;

namespace CSharpToGo.Core.Compiler
{
    public class Runner
    {
        private static readonly Runner _instance = new Runner();

        private Evaluator _eval;
        private List<string> _errors, _warnings;

        private const string _classDeclarationRegexPattern =
            @"^\s*((new|public|protected|internal|private|abstract|sealed)\s+)*class";
        private Regex _classDeclarationRegex;

        public RunnerOptions Options { get; set; }

        private Runner()
        {
            _classDeclarationRegex = new Regex(_classDeclarationRegexPattern, RegexOptions.CultureInvariant);

            Options = new RunnerOptions();

            ClearSession();

            MessageHub.Instance.Subscribe<AddNamespaceMessage>(message =>
                addUsing(message.Content));
        }

        public static Runner Instance
        {
            get { return _instance; }
        }

        // TODO: this function is a mess, clean it up
        public void RunAsync(string input, Action<ExecutionResult> callback = null)
        {
            _eval.Interrupt();

            _errors.Clear();
            _warnings.Clear();

            input = input.Trim();

            if (!input.EndsWith(";"))
                input += ";";

            var result = new ExecutionResult() { Input = input };

            // run the execution in tasks so we can set a timeout
            var cancelExecution = new CancellationTokenSource();
            var nonblockingTask = new Task(() =>
            {
                if (input.Trim().StartsWith("using ", StringComparison.InvariantCulture))
                {
                    _errors.Add(Options.UsingMessage);
                }
                else if (_classDeclarationRegex.IsMatch(input))
                {
                    _errors.Add(Options.ClassMessage);
                }
                else
                {
                    var runCode = new Task(() =>
                    {
                        var originalConsoleOut = Console.Out;

                        try
                        {
                            object evalResult;
                            bool gotResult;

                            using (var consoleOutStream = new MemoryStream())
                            {
                                using (var consoleOutWriter = new StreamWriter(consoleOutStream))
                                {
                                    Console.SetOut(consoleOutWriter);

                                    _eval.Evaluate(input, out evalResult, out gotResult);
                                    consoleOutWriter.Flush();

                                    result.ConsoleOutput = consoleOutWriter.Encoding.GetString(consoleOutStream.ToArray());
                                }
                            }

                            if (gotResult)
                            {
                                result.Output = evalResult.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            _errors.Add(e.GetType() + ": " + e.Message);
                        }
                        finally
                        {
                            Console.SetOut(originalConsoleOut);
                        }
                    }, cancelExecution.Token);

                    runCode.Start();

                    bool executionCompleted = runCode.Wait(Options.Timeout * 1000, cancelExecution.Token);

                    if (!executionCompleted)
                    {
                        _eval.Interrupt();
                        _errors.Add(Options.TimeoutMessage);
                    }
                }

                result.Errors = new List<string>(_errors);
                result.Warnings = new List<string>(_warnings);

                if (callback != null)
                    callback(result);
            });

            nonblockingTask.Start();
        }

        public IList<string> GetCodeCompletions(string input)
        {
            string prefix;

            return _eval.GetCompletions(input, out prefix) ?? new string[0];
        }

        public void ClearSession(Action callback = null)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                _errors = new List<string>();
                _warnings = new List<string>();

                var report = new Report(new Printer(msg => _errors.Add(msg.Text), msg => _warnings.Add(msg.Text)));
                var settings = new CommandLineParser(report).ParseArguments(new string[] { });
                _eval = new Evaluator(settings, report);

                foreach (string @namespace in Options.DefaultNamespaces)
                {
                    addUsing(@namespace);
                }

                if (callback != null)
                    callback();
            });
        }

        public IList<string> GetUsings()
        {
            var usings = _eval.GetUsing();

            if (usings == null)
                return new List<string>();

            return usings
                    .Split(new[] { ";" + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Substring(6))
                    .ToList();
        }

        public IList<Variable> GetVariables()
        {
            return _eval
                    .GetVars()
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(' '))
                    .Where(lineParts => lineParts.Count() >= 2)
                    .Select(lineParts => new Variable() 
                                             {
                                                 TypeName = lineParts[0],
                                                 Name = lineParts[1]
                                             })
                    // workaround for: https://bugzilla.novell.com/show_bug.cgi?id=705044
                    .Where(variable => !variable.Name.StartsWith("<>"))
                    .ToList();
        }

        private void addUsing(string @namespace)
        {
            _eval.Interrupt();
            _eval.Run(string.Format("using {0};", @namespace));
        }
    }
}