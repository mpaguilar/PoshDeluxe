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

namespace PoshManagerCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var pas = "password";
            var sec = new System.Security.SecureString();
            foreach( var c in pas.ToCharArray() ) {
                sec.AppendChar(c);
            }

            PSCredential creds = new PSCredential("administrator", sec);
            using (ManagerShell mgr = new ManagerShell())
            {
                var posh = mgr.GetPowerShell();
                DisplayAdapters(posh);
                System.Threading.Thread.Sleep(1000);
            }
        }
        async static void DisplayAdapters(PowerShell powerShell )
        {

            var netModule = new NetModule(
                powerShell,
                "vwin8"
                );

            var netWait = netModule.Refresh();

            Task.WaitAll(new[] {
                netWait
            });

            DisplayErrors(powerShell);

            var foo = netModule.NetworkAdapters;
            foreach (var f in foo)
            {
                Console.WriteLine(f);
            }

            foreach (var r in netModule.Routes)
            {
                Console.WriteLine(r);
            }
            
        }

        static void DisplayErrors(PowerShell powerShell)
        {
            if (powerShell.HadErrors)
            {
                ConsoleColor prev = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                var errors = powerShell.Streams.Error.Select((m) =>
                {
                    if (null != m.Exception)
                        return m.Exception.Message;
                    else
                        return "none";
                });
                foreach (var msg in errors)
                {
                    Console.WriteLine("ERROR: {0}", msg);
                    Console.WriteLine();
                }
                Console.ForegroundColor = prev;
                Console.WriteLine();
            }
        }

        static void DisplayMessages(NetModule net)
        {
            foreach (var msg in net.VerboseMessages)
            {
                Console.WriteLine("VERBOSE: {0}", msg);
            }
        }

        static void Meh()
        {
            var posh = new NetModule(
                    PowerShell.Create(),
                    "vwin8"
                   );
            var p = posh.Posh;

            p.AddStatement()
                .AddScript(String.Format(". \"{0}\"", @"scripts\GetNicInfo.ps1"));


            p.AddStatement()
                .AddCommand("Get-NetworkAdapter")
                .AddArgument("vwin8");



            var r = p.Invoke<ManagementObject>();
            // r[0].Properties["Caption"].Value
            Console.WriteLine(r.ToString());
        }

        static void Fizzy()
        {
            var interval = Observable.Interval(TimeSpan.FromMilliseconds(100));
            var fizz = interval.Where((val) =>
            {
                var ret = ((val % 5 == 0) && !(val % 10 == 0));
                return ret;
            });
            var buzz = interval.Where(val => val % 10 == 0);

            interval.Subscribe(
                x => Console.WriteLine("{0}", x),
                () => Console.WriteLine("Complete"));

            fizz.Subscribe(
                x => Console.WriteLine("Fizz: {0}", x),
                () => Console.WriteLine("Complete"));

            buzz.Subscribe(
                x => Console.WriteLine("Buzz: {0}", x),
                () => Console.WriteLine("Complete"));


            System.Threading.Thread.Sleep(5000);
        }
    }
}
