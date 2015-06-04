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

        private IEnumerable<ManagementObject> _nics = null;
        private IEnumerable<String> _routes = null;
        public IEnumerable<ManagementObject> NetworkAdapters
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

        private void init()
        {
            Posh.Commands.Clear();
            DotInclude("scripts\\GetNicInfo.ps1");
        }

        public Task Refresh()
        {
            return Task.Run(() => {
                RefreshNetworkAdapters();
                RefreshRoutes();                
            });
        }

        public void RefreshNetworkAdapters()
        {
            init();
            Posh.AddStatement()
                .AddCommand("Get-NetworkAdapter", true)
                .AddArgument(ComputerName);

            _nics = Posh.Invoke<ManagementObject>();
        }

        public void RefreshRoutes()
        {
            init();
            Posh.AddStatement()
                .AddCommand("Get-PersistentRoutes", true)
                .AddArgument(ComputerName);

            var rt = Posh.Invoke<String>();
            _routes = rt;
        }

    }
}
