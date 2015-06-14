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
    public class DiskModule : BasePoshModule
    {
        public class DiskDrive
        {
            public String DeviceID = String.Empty;
            public String FileSystem = String.Empty;
            public UInt64 Size = 0;
            public UInt64 FreeSpace = 0;
            public UInt32 MediaType = 0;
            public String Caption = String.Empty;
            public String Name = String.Empty;

            public override string ToString()
            {
                return String.Format("{0} {1} bytes available", DeviceID, FreeSpace);
            }
        }

        private IEnumerable<DiskDrive> _diskDrives = null;
        public IEnumerable<DiskDrive> DiskDrives
        {
            get { return _diskDrives; }
        }


        public DiskModule( 
            PowerShell powerShell,
            String computerName,
            String scriptPath = "scripts\\GetDiskInfo.ps1")
            : base(powerShell, computerName, scriptPath)
        {

        }

        public Task Refresh(IPoshStream stream)
        {
            return Task.Run(() =>
            {
                Posh.AddCommand(ScriptPath)
                    .AddParameter("ComputerName", ComputerName);

                var psoCollection = Invoke(stream);

                _diskDrives = psoCollection.Select(
                    pso => pso.ImmediateBaseObject as DiskDrive);
                
            });
        }

    }
}
