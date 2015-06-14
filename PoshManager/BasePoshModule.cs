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
        public readonly PowerShell Posh;

        public readonly String ComputerName;

        public void DotInclude(String script)
        {
            Posh
                .AddScript(String.Format(". \"{0}\"", script));
        }

        public BasePoshModule(PowerShell powerShell, String computerName)
        {
            Posh = powerShell;
            ComputerName = computerName;
        }

        public void ClearMessages()
        {
            Posh.Streams.ClearStreams();
        }
        public IEnumerable<String> VerboseMessages
        {
            get { return Posh.Streams.Verbose.Select(msg => msg.Message); }
        }
        public IEnumerable<String> WarningMessages
        {
            get { return Posh.Streams.Warning.Select(msg => msg.Message); }
        }

        public IEnumerable<String> ErrorMessages
        {
            get { return Posh.Streams.Error.Select(msg => msg.ErrorDetails.Message); }
        }
        public IEnumerable<String> DebugMessages
        {
            get { return Posh.Streams.Debug.Select(msg => msg.Message); }
        }

        public IEnumerable<String> GetPoshVariable(String variableName)
        {
            var bar = Posh.Runspace.SessionStateProxy.GetVariable(variableName);

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
            var poshWait = Posh.BeginInvoke();
            bool hasCounted = false;
            while (!poshWait.IsCompleted)
            {
                // I'm not sure why I'm having to do this,
                // but if I don't get the count right off the stream things
                // don't work as well.
                if (!hasCounted && (
                    Posh.Streams.Verbose.Count > 0 ||
                    Posh.Streams.Debug.Count > 0 ||
                    Posh.Streams.Warning.Count > 0 ||
                    Posh.Streams.Error.Count > 0) )
                {
                    hasCounted = true;
                }

                if (hasCounted)
                {
                    WriteMessages(VerboseMessages, stream.VerboseWriter);
                    Posh.Streams.Verbose.Clear();

                    WriteMessages(WarningMessages, stream.WarningWriter);
                    Posh.Streams.Warning.Clear();

                    WriteMessages(DebugMessages, stream.DebugWriter);
                    Posh.Streams.Debug.Clear();

                    WriteMessages(ErrorMessages, stream.ErrorWriter);
                }
            }

            var ret = Posh.EndInvoke(poshWait);

            WriteMessages(VerboseMessages, stream.VerboseWriter);
            WriteMessages(WarningMessages, stream.WarningWriter);
            WriteMessages(DebugMessages, stream.DebugWriter);
            WriteMessages(ErrorMessages, stream.ErrorWriter);

            return ret;
        }

        public PSDataCollection<PSObject> Invoke(Action<String> msgOut)
        {
            var poshWait = Posh.BeginInvoke();

            if (null != msgOut)
            {
                while (!poshWait.IsCompleted)
                {
                    // we don't want to clear the stream unless
                    // there's been something to process
                    // otherwise, messages are dropped.
                    if (Posh.Streams.Verbose.Count > 0)
                    {
                        foreach (var msg in VerboseMessages)
                            msgOut(msg);

                        Posh.Streams.Verbose.Clear();
                    }
                    // surrender the thread...
                    System.Threading.Thread.Sleep(1);
                }
            }

            return Posh.EndInvoke(poshWait);
        }
    }
}
