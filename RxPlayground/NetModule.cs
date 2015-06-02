using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Linq;

using System.Management;
using System.Management.Automation;


namespace RxPlayground
{
    public class NetModule
    {
        public readonly PowerShell Posh;

        private IEnumerable<ManagementObject> _nics = null;
        public IObservable<ManagementObject> NetworkAdapters
        {
            get { return _nics.ToObservable<ManagementObject>(); }
        }
        public IEnumerable<ManagementObject> RawNics
        {
            get {
                if (_nics == null)
                {
                    GetNetworkAdapters();
                }
                return _nics;  
            }
        }

        public NetModule()
        {
            Posh = PowerShell.Create();
            DotInclude(@"scripts\GetNicInfo.ps1");
            
        }

        public void DotInclude( String script)
        {
            Posh.AddStatement()
                .AddScript(String.Format(". \"{0}\"", script));

        }

        public IEnumerable<String> RunScript(String script)
        {
            Posh.AddScript(script);
            var res = Posh.Invoke();

            Posh.Commands.Clear();
            return res.Select( r => r.ToString());
        }

        public void ClearMessages()
        {
            Posh.Streams.Error.Clear();
            Posh.Streams.Warning.Clear();
        }

        public void GetNetworkAdapters()
        {
            Posh.AddStatement()
                .AddCommand("Get-NetworkAdapter", true)
                .AddArgument("vwin8");

            var r = Posh.Invoke<ManagementObject>();
            _nics = r;
        }

        public IObservable<String> Verbose()
        {
            return Posh.Streams.Verbose.Select(msg => msg.Message).ToObservable();
        }

        public IEnumerable<String> Warnings()
        {
            return Posh.Streams.Warning.Select(warn => warn.Message);
        }

        public IEnumerable<String> Errors()
        {
            return Posh.Streams.Error.Select(err => err.ErrorDetails.Message);
        }
    }
}
