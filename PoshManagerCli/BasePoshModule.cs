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

        public readonly String ComputerName;

        public void DotInclude(String script)
        {
            Posh
                .AddScript(String.Format(". \"{0}\"", script));
        }

        public BasePoshModule(PowerShell powerShell, String computerName )
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
    }
}
