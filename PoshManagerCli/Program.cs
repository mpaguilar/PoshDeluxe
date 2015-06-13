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

            using (ManagerShell mgr = new ManagerShell())
            using(var posh = mgr.GetPowerShell())
            {

                var netModule = new NetModule(
                    posh,
                    "grunt"
                    );

                var netWait = netModule.Refresh(cw);

                Task.WaitAll(new[] { netWait });

                foreach (var f in netModule.NetworkAdapters)
                {
                    Console.WriteLine(f);
                }
            }
        }
    }
}
