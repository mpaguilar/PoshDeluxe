using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Linq;
using System.Reactive.Subjects;

using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.Commands.Management;
using PowerShell = System.Management.Automation.PowerShell;

using PoshManager;

namespace PoshManagerCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var cw = new ConsoleWriter();
            var computerName = "grunt";

            using (ManagerShell mgr = new ManagerShell())
            using(var netPosh = mgr.GetPowerShell())
            using(var diskPosh = mgr.GetPowerShell())
            {

                var netModule = new NetModule(
                    netPosh,
                    computerName
                    );

                var diskModule = new DiskModule(
                    diskPosh,
                    computerName
                    );

                var netWait = netModule.Refresh(new ConsoleWriter());
                var diskWait = diskModule.Refresh(new ConsoleWriter());

                Task.WaitAll(new[] { netWait, diskWait });

                foreach (var f in netModule.NetworkAdapters)
                {
                    Console.WriteLine(f);
                }

                foreach ( var d in diskModule.DiskDrives )
                {
                    Console.WriteLine(d);
                }
            }
        }

        static Action<Task> Refresher(IPoshModule module, String computerName)
        {
            return (writer) =>
            {

            };
        }
    }
}
