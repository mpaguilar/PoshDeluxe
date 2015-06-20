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
            {

                var netModule = new NetModule(
                    mgr.GetPowerShell(),
                    computerName
                    );

                var diskModule = new DiskModule(
                    mgr.GetPowerShell(),
                    computerName
                    );


                Func<IPoshModule, PowerShell> posh = (mod) => {
                    return mgr.GetPowerShell();
                };
                
                var netWait = Refresher(netModule)(new ConsoleWriter());
                var diskWait = Refresher(diskModule)(new ConsoleWriter());

                Task.WaitAll(new[] { netWait, diskWait });

                foreach (var f in netModule.NetworkAdapters)
                {
                    Console.WriteLine(f);
                }
            }
        }

        static Func<Type,IPoshModule> DiskModule(String computerName)
        {
            return (moduleType) =>
            {
                return new DiskModule(computerName);
            };
        }

        static Func<PowerShell, IPoshModule> Module(Type moduleType)
        {
            var ctor = moduleType.GetConstructor(new[] { typeof(PowerShell) });

            if (null == ctor)
            {
                throw (new ArgumentException("Constructor not found"));
            }
            
            return (shell) =>
            {
                return (IPoshModule)ctor.Invoke(
                    new object[] {shell});
            };
        }

        static Func<IPoshStream, Task> Refresher(IPoshModule module)
        {
            return (writer) =>
            {
                return module.Refresh(writer);
            };
        }
    }
}
