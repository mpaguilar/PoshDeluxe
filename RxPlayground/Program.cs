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

namespace RxPlayground
{
    class Program
    {
        static void Main(string[] args)
        {

            InitialSessionState defaultIss = InitialSessionState.CreateDefault();
            InitialSessionState cleanIss = InitialSessionState.Create();

            cleanIss.LanguageMode = PSLanguageMode.FullLanguage;

            MigrateCommands(cleanIss, defaultIss, new[] {
                "Get-WmiObject",
                "Write-Verbose",
                "Write-Debug",
                "Write-Warning",
                "Invoke-Command",
                "Get-Item",
                "Select-Object"
            });

            MigrateVariables(cleanIss, defaultIss);

            // var runPool = RunspaceFactory.CreateRunspacePool(cleanIss);

            using (Runspace rs = RunspaceFactory.CreateRunspace(defaultIss))
            {
                rs.Open();
                using( PowerShell posh = PowerShell.Create() ) {
                    // posh.Runspace = rs;
                    OldSkool(posh);
                }
            }

        }

        static void MigrateProviders( InitialSessionState clean,
            InitialSessionState source)
        {
            foreach (var p in source.Providers)
            {
                clean.Providers.Add(p);
            }
        }

        static void MigrateVariables(InitialSessionState clean,
            InitialSessionState source)
        {
            foreach (var svar in source.Variables)
            {
                clean.Variables.Add(svar);
            }
        }

        static void MigrateCommands(InitialSessionState clean, 
            InitialSessionState source, 
            String[] commands)
        {
            foreach (var cmd in commands)
            {
                var check_cmd = clean.Commands.Where(c => c.Name == cmd);
                if( 0 < check_cmd.Count() )
                {
                    var msg = String.Format("Command already in session state: {0}", cmd);
                    Console.WriteLine(msg);
                }

                var scmd = source.Commands.Where(c => c.Name == cmd).First();
                var scmdCopy = (SessionStateCommandEntry)scmd.Clone();
                clean.Commands.Add(scmdCopy);
            }
        }

        

        async static void OldSkool(PowerShell powerShell )
        {

            var netModule = new NetModule(
                powerShell,
                "vwin8"
                );

            var netWait = netModule.Refresh();
            

            Task.WaitAll(new[] {
                netWait
            });

            // System.Threading.Thread.Sleep(1000);
            if (powerShell.HadErrors)
            {
                ConsoleColor prev = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                var errors = powerShell.Streams.Error.Select((m) => {
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
                return;
            }

            var foo = netModule.NetworkAdapters.Select(mo => mo.Properties["Caption"].Value);
            foreach (var f in foo)
            {
                Console.WriteLine(f);
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
