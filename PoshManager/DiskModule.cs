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
    public class DiskModule : BasePoshModule, IPoshModule
    {
        public const String _scriptPath = "scripts\\GetDiskInfo.ps1";
        public class DiskDrive
        {
            // WPF requires getters and setters

            public String DeviceID  {get; set;}
            public String FileSystem { get; set; }
            public UInt64 Size { get; set; }
            public UInt64 FreeSpace { get; set; }
            public UInt32 MediaType { get; set; }
            public String Caption { get; set; }
            public String Name { get; set; }

            public override string ToString()
            {
                return String.Format("{0} {1} Gb available", DeviceID, (FreeSpace/(1024 * 1024 * 1024)));
            }
        }

        private IEnumerable<DiskDrive> _diskDrives = null;
        public IEnumerable<DiskDrive> DiskDrives
        {
            get { return _diskDrives; }
        }


        public DiskModule( 
            PowerShell powerShell,
            String computerName)
            : base(powerShell, computerName, _scriptPath)
        {

        }

        public DiskModule(
            PowerShell powerShell ) :
            base(powerShell, _scriptPath)
        {

        }

        public DiskModule(
            String computerName
            )
            : base(null, _scriptPath) { }

        public Task Refresh(IPoshStream stream)
        {
            return Task.Run(() =>
            {
            //    Shell.AddStatement()
            //        .AddCommand(_scriptPath)
            //        .AddParameter("ComputerName", ComputerName);

                var psoCollection = Invoke(stream);

                _diskDrives = psoCollection.Select(
                    pso => pso.ImmediateBaseObject as DiskDrive);
                
            });
        }

    }
}
