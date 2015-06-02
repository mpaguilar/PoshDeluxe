using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Subjects;

using System.Management;
using System.Management.Automation;

namespace RxPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            var posh = new NetModule();
            var verbose = new Subject<String>();
            var meh = posh.RawNics;
            var foo = meh.Select(mo => mo.Properties["Caption"].Value );
            foreach( var f in foo ) {
                Console.WriteLine(f);
            }
        }
        
        static void WriteVerbose(IObservable<String> verboseMessages)
        {
            verboseMessages.Subscribe(Console.WriteLine);
        }

        static void Meh()
        {
            var posh = new NetModule();
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
    }
}
