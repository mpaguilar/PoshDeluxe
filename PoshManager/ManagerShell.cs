using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.Commands.Management;
using PowerShell = System.Management.Automation.PowerShell;

namespace PoshManager
{
    public class ManagerShell : IDisposable
    {
        Runspace Runspace;

        public ManagerShell()
        {
            this.Runspace = RunspaceFactory.CreateRunspace(
                GetInitialSessionState());
            this.Runspace.Open();
        }

        public void Dispose()
        {
            if (null != this.Runspace)
            {
                this.Runspace.Close();
                this.Runspace.Dispose();
                this.Runspace = null;
            }
        }

        public PowerShell GetPowerShell()
        {
            var ret = PowerShell.Create();
            ret.Runspace = this.Runspace;
            return ret;
        }

        private InitialSessionState GetInitialSessionState()
        {
            InitialSessionState defaultIss = InitialSessionState.CreateDefault();
            InitialSessionState cleanIss = InitialSessionState.Create();

            cleanIss.LanguageMode = PSLanguageMode.FullLanguage;

            //SessionStateTypeEntry meh = new SessionStateTypeEntry()

            MigrateCommands(cleanIss, defaultIss, new[] {
                // "*" gets two functions 
                // one of them does dot-sourcing
                // the other allows scripts as commands (I think)
                "*", 
                "Get-WmiObject",
                "Write-Host",
                "Write-Verbose",
                "Write-Debug",
                "Write-Warning",
                "Add-Type",
                "ForEach-Object",
                "New-Object",
                "Where-Object",
//                "Invoke-Command",
//                "Get-Item",
                "Select-Object"
            });

            //MigrateVariables(cleanIss, defaultIss);
            cleanIss.Variables.Add(new SessionStateVariableEntry("VerbosePreference", "Continue", ""));

            MigrateProviders(cleanIss, defaultIss, new String[] {
                "Function",
                "Variable",
//                "Environment",
//                "Alias",
//                "WSMan",
//                "Certificate",
//                "Registry",
                "FileSystem"
            });

            //MigrateAssemblies(cleanIss, defaultIss);
            //MigrateFormats(cleanIss, defaultIss);



            return cleanIss;

        }

        static void MigrateAssemblies(InitialSessionState clean,
    InitialSessionState source)
        {

            foreach (var a in source.Assemblies)
            {
                clean.Assemblies.Add(a);
            }
        }

        static void MigrateFormats(InitialSessionState clean,
            InitialSessionState source)
        {

            foreach (var f in source.Formats)
            {
                clean.Formats.Add(f);
            }

        }
        static void MigrateProviders(InitialSessionState clean,
            InitialSessionState source,
            String[] providers = null)
        {

            foreach (var p in source.Providers)
            {
                if (null == providers || providers.Contains(p.Name))
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
                if (0 < check_cmd.Count())
                {
                    var msg = String.Format("Command already in session state: {0}", cmd);
                    Console.WriteLine(msg);
                }

                var scmds = source.Commands.Where(c => c.Name == cmd);
                foreach (var scmd in scmds)
                {
                    var scmdCopy = (SessionStateCommandEntry)scmd.Clone();
                    clean.Commands.Add(scmdCopy);
                }
            }
        }
    }
}
