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

        private void init()
        {
            Posh.Commands.Clear();
            // DotInclude("scripts\\GetNicInfo.ps1");
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

                ExtractResults(ret);

                _nics = GetPoshVariable("NetworkAdapters");
                _routes = GetPoshVariable("PersistentRoutes");

                Console.WriteLine("meh");
                
            });

            t.ContinueWith((task) => Console.WriteLine("Finished"));

            return t;
        }

        private IEnumerable<String> ExtractResults(Collection<PSObject> resultObjects )
        {
            foreach (var o in resultObjects)
            {
                var meh = o.Properties["NetworkAdapters"].Value;
            }

            return new List<String>();
        }

        private IEnumerable<String> GetPoshVariable(String variableName)
        {
            var bar = Posh.Runspace.SessionStateProxy.GetVariable(variableName);
            var foo = bar as object[];

            if( null == foo )
            {
                
            }

            if (null != foo)
                return new List<String>(foo.Select(o => o.ToString()));
            else
                return new String[0];
        }

        /// <summary>
        /// ///////////
        /// </summary>

        public void RefreshNetworkAdapters()
        {
            init();
            Posh.AddStatement()
                .AddCommand("Get-NetworkAdapter", true)
                .AddArgument(ComputerName);


            var meh = Posh.Invoke<ManagementObject>();
            
            
            Console.WriteLine("meh");
        }

        public void RefreshRoutes()
        {
            init();
            Posh.AddStatement()
                .AddCommand("Get-PersistentRoutes", true)
                .AddArgument(ComputerName);

            var rt = Posh.Invoke<String>();
            // _routes = rt;
        }

    }
}
