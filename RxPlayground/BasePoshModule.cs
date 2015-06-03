using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

namespace PoshManagerCli
{
    public class BasePoshModule
    {
        public readonly PowerShell Posh;
        public void DotInclude(String script)
        {
            Posh
                .AddScript(String.Format(". \"{0}\"", script));
        }

        public BasePoshModule(PowerShell powerShell )
        {
            Posh = powerShell;
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
    }
}
