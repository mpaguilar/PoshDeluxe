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

        public NetModule(PowerShell powerShell, String computerName)
            : base( powerShell, computerName )
        {

        }

        public Task Refresh()
        { 
            Task t = 
            Task.Run(() =>
            {
                Posh.Commands.Clear();

                Posh.AddCommand("scripts\\GetNicInfo.ps1")
                    .AddParameter("ComputerName", ComputerName);

                var ret = Posh.Invoke();

                _nics = GetPoshVariable("NetworkAdapters");
                _routes = GetPoshVariable("PersistentRoutes");
                
            });

            return t;
        }
    }
}
