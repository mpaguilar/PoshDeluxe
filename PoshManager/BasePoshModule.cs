using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

namespace PoshManager
{
    public class BasePoshModule
    {
        public readonly PowerShell Shell;

        public readonly String ComputerName;
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
            get { return Shell.Streams.Error.Select(msg => msg.ErrorDetails.Message); }
        }
        public IEnumerable<String> DebugMessages
        {
            get { return Shell.Streams.Debug.Select(msg => msg.Message); }
        }

        public IEnumerable<String> GetPoshVariable(String variableName)
        {
            var bar = Shell.Runspace.SessionStateProxy.GetVariable(variableName);

            if (null != bar as object[])
            {
                var foo = bar as object[];
                return new List<String>(foo.Select(o => o.ToString()));
            }

            if (null != bar as String)
                return new List<String>(new[] { bar as String });

            return new String[0];
        }
        public void WriteMessages(IEnumerable<String> messages, Action<String> writer)
        {
            // TODO: fix this
            // the problem with this approach is that not all the messages
            // will not be properly interleaved
            foreach (var msg in messages)
            {
                writer(msg);
            }
        }
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

        public PSDataCollection<PSObject> Invoke(Action<String> msgOut)
        {
            var poshWait = Shell.BeginInvoke();

            if (null != msgOut)
            {
                while (!poshWait.IsCompleted)
                {
                    // we don't want to clear the stream unless
                    // there's been something to process
                    // otherwise, messages are dropped.
                    if (Shell.Streams.Verbose.Count > 0)
                    {
                        foreach (var msg in VerboseMessages)
                            msgOut(msg);

                        Shell.Streams.Verbose.Clear();
                    }
                    // surrender the thread...
                    System.Threading.Thread.Sleep(1);
                }
            }

            return Shell.EndInvoke(poshWait);
        }
    }
}
