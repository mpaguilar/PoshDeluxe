using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace PoshManager
{

    public class NetModule : BasePoshModule
    {
        public class NetworkAdapter
        {
            public Int32 Id = -1;
            public String MACAddress = String.Empty;
            public Int32 MaxSpeed = -1;
            public bool NetEnabled = false;
            public String Description = String.Empty;
            public List<IPAddress> IPAddresses = new List<IPAddress>();

            public override string ToString()
            {
                return String.Format("{0}: {1}", Id, Description);
            }
        }

        public class IPAddress
        {

            public String Address = String.Empty;
            public String SubnetMask = String.Empty;

            public override String ToString()
            {
                return String.Format("{0}/{1}", Address, SubnetMask);
            }
        }


        private IEnumerable<NetworkAdapter> _nics = null;
        public IEnumerable<NetworkAdapter> NetworkAdapters
        {
            get { return _nics; }
        }

        public readonly String ScriptPath;

        public NetModule(
            PowerShell powerShell, 
            String computerName,
            String scriptPath = "scripts\\GetNicInfo.ps1")
            : base(powerShell, computerName)
        {
            ScriptPath = scriptPath;
        }

        public Task Refresh(IPoshStream stream)
        {

            Task t =
            Task.Run(() =>
            {
                Posh.AddCommand(ScriptPath)
                    .AddParameter("ComputerName", ComputerName);

                var psoCollection = Invoke(stream);

                _nics = psoCollection.Select(pso => pso.ImmediateBaseObject as NetworkAdapter);

            });

            return t;
        }
    }
}

