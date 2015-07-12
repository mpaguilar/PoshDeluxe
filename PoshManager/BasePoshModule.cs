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

            Shell
                .AddCommand(ScriptPath)
                .AddParameter("ComputerName", ComputerName);

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

        public void WritePoshMessages(IPoshStream stream, 
            IEnumerable<String> messages, 
            PoshMessage.MessageType messageType)
        {
            // TODO: fix this
            // the problem with this approach is that the messages
            // will not be properly interleaved
            foreach (var msg in messages)
            {
                stream.Write(
                    new PoshMessage { Message = msg, Type = messageType }
                    );
            }
        }

        public void WriteStreams(IPoshStream stream)
        {
            WritePoshMessages(stream,
                VerboseMessages, PoshMessage.MessageType.Verbose);

            Shell.Streams.Verbose.Clear();

            WritePoshMessages(stream,
                WarningMessages, PoshMessage.MessageType.Warning);
            Shell.Streams.Warning.Clear();

            WritePoshMessages(stream,
                DebugMessages, PoshMessage.MessageType.Debug);
            Shell.Streams.Debug.Clear();


            // the default ErrorPreference is to Stop
            // TODO: this will spew dupes if ErrorPreference is not Stop
            WritePoshMessages(stream,
                ErrorMessages, PoshMessage.MessageType.Error);
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
                System.Threading.Thread.Sleep(1);
                // I'm not sure why I'm having to do this,
                // but if I don't get a count the first time
                // data is available, I don't ever get anything. Hmph!
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
                    WriteStreams(stream);
                }
                
            }
            
            var ret = Shell.EndInvoke(poshWait);

            // any left?
            WriteStreams(stream);

            return ret;
        }

        #region IDispose members
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
        #endregion
    }
}
