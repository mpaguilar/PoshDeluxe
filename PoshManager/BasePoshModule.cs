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

        public PSDataCollection<PSObject> Invoke(IPoshStream stream)
        {
            var poshWait = Posh.BeginInvoke();

            while (!poshWait.IsCompleted)
            {
                // we don't want to clear the stream unless
                // there's been something to process
                // otherwise, messages are dropped.
                if (Posh.Streams.Verbose.Count > 0)
                {
                    foreach (var msg in VerboseMessages)
                        stream.VerboseWriter(msg);

                    Posh.Streams.Verbose.Clear();
                }

                if(Posh.Streams.Warning.Count > 0 )
                {
                    foreach( var msg in WarningMessages )
                    {
                        stream.WarningWriter(msg);
                    }

                    Posh.Streams.Warning.Clear();
                }

                if (Posh.Streams.Debug.Count > 0)
                {
                    foreach (var msg in DebugMessages)
                    {
                        stream.DebugWriter(msg);
                    }

                    Posh.Streams.Debug.Clear();
                }

                if (Posh.Streams.Error.Count > 0)
                {
                    foreach (var msg in ErrorMessages)
                    {
                        stream.ErrorWriter(msg);
                    }

                    // Posh.Streams.Error.Clear();
                }
                // surrender the thread...
                System.Threading.Thread.Sleep(1);
            }

            return Posh.EndInvoke(poshWait);
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
