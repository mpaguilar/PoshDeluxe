using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

namespace PoshManager
{
    public class AdModule : BasePoshModule
    {
        public const String _scriptPath = "scripts\\GetAdInfo.ps1";
        public AdModule(
            PowerShell powerShell,
            String computerName)
            : base(powerShell, computerName, _scriptPath) { 
        
        }

        public List<object> Results = new List<object>();

        public Task Refresh(IPoshStream stream) {
            return Task.Run(() =>
            {
                //    Shell.AddStatement()
                //        .AddCommand(_scriptPath)
                //        .AddParameter("ComputerName", ComputerName);

                var psoCollection = Invoke(stream);

                var foo = psoCollection.Select(
                    duh =>
                    {
                        return new { 
                            Name = duh.Members["Name"].Value,
                            DNSHostName = duh.Members["DNSHostName"].Value,
                            Ping = duh.Members["Ping"].Value                        
                        };
                    });

                Results.AddRange(foo);

                var ret = psoCollection;

            });
        }
    }
}
