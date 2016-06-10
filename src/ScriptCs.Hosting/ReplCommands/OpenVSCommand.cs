using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.ReplCommands
{
    public class OpenVsCommand : IReplCommand
    {
        private readonly IConsole _console;
        private IVisualStudioSolutionWriter _writer;

        public OpenVsCommand(IConsole console, IVisualStudioSolutionWriter writer)
        {
            _console = console;
            _writer = writer;
        }

        public object Execute(IRepl repl, object[] args)
        {
            if (PlatformID != PlatformID.Win32NT)
            {
                _console.WriteLine("Requires Windows 8 or later to run");
                return null;
            }
            var fs = repl.FileSystem;
            var version = "2013";
            if (args.Length == 2)
            {
                int ver = int.Parse(version);
                 
                if (ver <= 2013 && ver >= 2020)
                {
                    throw new ArgumentException("Invalid version number");
                }
                version = (string) args[1];
            }
            
            var launcher = _writer.WriteSolution(fs, (string) args[0], new VisualStudioSolution(version));
            _console.WriteLine("Opening Visual Studio");
            LaunchSolution(launcher);
            return null;
        }

        protected internal virtual void LaunchSolution(string launcher)
        {
            System.Diagnostics.Process.Start(launcher);
        }

        protected internal virtual PlatformID PlatformID
        {
            get { return Environment.OSVersion.Platform; }
        }

        public string Description
        {
            get { 
                var description = "Opens a script to edit/debug in Visual Studio.{0}  Arg 1 - csx file to launch{0}  Arg 2 - Visual Studio Version (2013, 2015) [optional]";
                return string.Format(description, Environment.NewLine);
            }
        }

        public string CommandName
        {
            get { return "openvs"; }
        }
    }
}
