using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

namespace PoshManager
{
    public class BasePoshModule : IDisposable
    {
        protected const String _computerName = "localhost";
        public readonly PowerShell Shell;

        public String ComputerName { get; protected set; }
        public readonly String ScriptPath;

        public void DotInclude(String script)
        {
            Shell
                .AddScript(String.Format(". \"{0}\"", script));
        }

        public BasePoshModule(
            PowerShell powerShell, 
            String computerName,
            String scriptPath)
        {
            Shell = powerShell;
            ComputerName = computerName;
            ScriptPath = scriptPath;

            var env = Environment.CurrentDirectory;            
        }

        public BasePoshModule(
            PowerShell powerShell,
            String scriptPath
            ): this(powerShell, _computerName, scriptPath )
        {
            Shell = powerShell;
            ScriptPath = scriptPath;
            ComputerName = _computerName;
        }

        public void ClearMessages()
        {
            Shell.Streams.ClearStreams();
        }
        public IEnumerable<String> VerboseMessages
        {
            get { return Shell.Streams.Verbose.Select(msg => msg.Message); }
        }
        public IEnumerable<String> WarningMessages
        {
            get { return Shell.Streams.Warning.Select(msg => msg.Message); }
        }

        public IEnumerable<String> ErrorMessages
        {
            get { return Shell.Streams.Error.Select(msg => msg.Exception.Message); }
        }
        public IEnumerable<String> DebugMessages
        {
            get { return Shell.Streams.Debug.Select(msg => msg.Message); }
        }

        // even more stuff can be pulled from global variables
        public IEnumerable<String> GetPoshVariable(String variableName)
        {
            var bar = Shell.Runspace.SessionStateProxy.GetVariable(variableName);

            // lists are objects
            if (null != bar as object[])
            {
                var foo = bar as object[];
                return new List<String>(foo.Select(o => o.ToString()));
            }

            // while single results are strings
            if (null != bar as String)
                return new List<String>(new[] { bar as String });

            return new String[0];
        }
        public void WriteMessages(IEnumerable<String> messages, Action<String> writer)
        {
            // TODO: fix this
            // the problem with this approach is that the messages
            // will not be properly interleaved

            foreach (var msg in messages)
            {
                writer(msg);
            }
        }

        // this does most of the work
        // fires off the command task async and
        // spits out the messages as they come
        public PSDataCollection<PSObject> Invoke(IPoshStream stream)
        {
            var poshWait = Shell.BeginInvoke();
            bool hasCounted = false;
            while (!poshWait.IsCompleted)
            {
                // I'm not sure why I'm having to do this,
                // but I only have to do it once
                if (!hasCounted && (
                    Shell.Streams.Verbose.Count > 0 ||
                    Shell.Streams.Debug.Count > 0 ||
                    Shell.Streams.Warning.Count > 0 ||
                    Shell.Streams.Error.Count > 0) )
                {
                    hasCounted = true;
                }

                if (hasCounted)
                {
                    WriteMessages(VerboseMessages, stream.VerboseWriter);
                    Shell.Streams.Verbose.Clear();

                    WriteMessages(WarningMessages, stream.WarningWriter);
                    Shell.Streams.Warning.Clear();

                    WriteMessages(DebugMessages, stream.DebugWriter);
                    Shell.Streams.Debug.Clear();

                    // the default is to Stop
                    // TODO: this will spew dupes if ErrorPreference is not Stop
                    WriteMessages(ErrorMessages, stream.ErrorWriter);
                }
            }
            
            var ret = Shell.EndInvoke(poshWait);

            // any left?
            WriteMessages(VerboseMessages, stream.VerboseWriter);
            WriteMessages(WarningMessages, stream.WarningWriter);
            WriteMessages(DebugMessages, stream.DebugWriter);
            WriteMessages(ErrorMessages, stream.ErrorWriter);

            return ret;
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                Shell.Dispose();
            }
            isDisposed = true;
        }

        ~BasePoshModule()
        {
            Dispose(false);
        }
    }
}
