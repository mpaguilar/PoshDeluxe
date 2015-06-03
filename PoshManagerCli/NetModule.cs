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
        public readonly String ComputerName;

        public NetModule(PowerShell powerShell, String computerName)
            : base( powerShell )
        {
            ComputerName = computerName;
            var path = System.IO.Path.Combine(new [] {
                Environment.CurrentDirectory,
                "scripts\\GetNicInfo.ps1"
            });
            DotInclude("scripts\\GetNicInfo.ps1");
        }

        public Task Refresh()
        {
            return Task.Run(() => {
                RefreshNetworkAdapters();
                // RefreshRoutes();
            });
        }

        public void RefreshNetworkAdapters()
        {
            Posh.AddStatement()
                .AddCommand("Get-NetworkAdapter", true)
                .AddArgument(ComputerName);

            _nics = Posh.Invoke<ManagementObject>();
        }

        public void RefreshRoutes()
        {
            Posh.AddStatement()
                .AddCommand("Get-PersistentRoutes", true)
                .AddArgument(ComputerName);

            _routes = Posh.Invoke<String>();
        }

    }
}
