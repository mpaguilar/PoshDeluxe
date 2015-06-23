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

    public class NetModule 
        : BasePoshModule, IPoshModule
    {
        public class NetworkAdapter
        {
            public Int32 Id { get; set; }
            public String MACAddress {get;set;}
            public Int32 MaxSpeed {get;set;}
            public bool NetEnabled {get;set;}
            public String Description {get;set;}
            public List<IPAddress> IPAddresses = new List<IPAddress>();

            public override string ToString()
            {
                return String.Format("{0}: {1}", Id, Description);
            }

            public NetworkAdapter()
            {

            }
        }

        public class IPAddress
        {
            // using strings is lazy...so I'm lazy
            public String Address {get; set;}
            public String SubnetMask {get;set;}

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

        public NetModule(
            PowerShell powerShell, 
            String computerName,
            String scriptPath = "scripts\\GetNicInfo.ps1")
            : base(powerShell, computerName, scriptPath)
        {
            
        }

        public Task Refresh(IPoshStream stream)
        {

            Task t =
            Task.Run(() =>
            {
                var psoCollection = Invoke(stream);

                _nics = psoCollection.Select(
                    pso => pso.ImmediateBaseObject as NetworkAdapter);

            });

            return t;
        }
    }
}

