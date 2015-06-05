using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace PoshManagerCli
{
    public class NetModule : BasePoshModule
    {

        private IEnumerable<String> _nics = null;
        private IEnumerable<String> _routes = null;
        public IEnumerable<String> NetworkAdapters
        {
            get { return _nics; }
        }

        public IEnumerable<String> Routes { get { return _routes; } }

        public readonly String ComputerName;

        public NetModule(PowerShell powerShell, String computerName)
            : base( powerShell )
        {
            ComputerName = computerName;
        }

        public Task Refresh()
        { 
            Task t = 
            Task.Run(() =>
            {
                Posh.Commands.Clear();

                Posh.AddScript(
                    String.Format("scripts\\GetNicInfo.ps1 -ComputerName {0}", ComputerName));

                var ret = Posh.Invoke();

                _nics = GetPoshVariable("NetworkAdapters");
                _routes = GetPoshVariable("PersistentRoutes");
                
            });

            return t;
        }

        private IEnumerable<String> GetPoshVariable(String variableName)
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
