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
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PoshManager
{
    public class ManagerShell : IDisposable
    {
        RunspacePool RunspacePool;
        public ManagerShell()
        {
            var iss = GetInitialSessionState();
            this.RunspacePool = RunspaceFactory.CreateRunspacePool(iss);
            this.RunspacePool.SetMinRunspaces(10);
            this.RunspacePool.SetMaxRunspaces(20);

            // this may not be necessary
            this.RunspacePool.ThreadOptions = PSThreadOptions.UseNewThread;
            this.RunspacePool.ApartmentState = System.Threading.ApartmentState.MTA;
            this.RunspacePool.Open();
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose( bool disposing )
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                if (null != this.RunspacePool)
                {
                    this.RunspacePool.Close();
                    this.RunspacePool.Dispose();
                    this.RunspacePool = null;
                }
            }

            isDisposed = true;
        }

        ~ManagerShell()
        {
            Dispose(false);
        }

        public PowerShell GetPowerShell()
        {
            var ret = PowerShell.Create(RunspaceMode.NewRunspace);
            ret.RunspacePool = this.RunspacePool;
            return ret;
        }

        private InitialSessionState GetInitialSessionState()
        {
            InitialSessionState defaultIss = InitialSessionState.CreateDefault();
            InitialSessionState cleanIss = InitialSessionState.Create();

            // this isn't restrictive.
            // this isn't restrictive at all!
            cleanIss.LanguageMode = PSLanguageMode.FullLanguage;            

            // Disable execution policy
            cleanIss.AuthorizationManager = new AuthorizationManager("dummy"); 

            // TODO: create the objects, rather than copying
            // is there something wrong with copying?
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
                "Get-Item",
                "Select-Object"
            });

            // Some output defaults.
            AddVariable(cleanIss, "VerbosePreference", "Continue");
            AddVariable(cleanIss, "WarningPreference", "Continue");
            AddVariable(cleanIss, "DebugPreference", "Continue");
            AddVariable(cleanIss, "ErrorPreference", "Stop");

            MigrateProviders(cleanIss, defaultIss, new String[] {
                "Function",
                "Variable",
//                "Environment",
//                "Alias",
//                "WSMan",
//                "Certificate",
//                "Registry",
                "FileSystem" // for dot sourcing
            });

            //MigrateAssemblies(cleanIss, defaultIss);
            //MigrateFormats(cleanIss, defaultIss);

            return cleanIss;

        }

        static void AddVariable( InitialSessionState sessionState, String variableName, String value )
        {
            sessionState.Variables.Add(new SessionStateVariableEntry(variableName, value, ""));
        }

        // yes, this is a terrible way to do it.
        // either 
        static void MigrateAssemblies(InitialSessionState clean,
            InitialSessionState source)
        {
            clean.Assemblies.Add(source.Assemblies.Clone());
        }

        static void MigrateFormats(InitialSessionState clean,
            InitialSessionState source)
        {
            clean.Formats.Add(source.Formats.Clone());
        }

        static void MigrateProviders(InitialSessionState clean,
            InitialSessionState source,
            String[] providers = null)
        {
            // this one blows up if you aren't careful
            foreach (var p in source.Providers)
            {
                if (null == providers || providers.Contains(p.Name))
                    clean.Providers.Add((SessionStateProviderEntry)p.Clone());
            }
        }

        static void MigrateVariables(InitialSessionState clean,
            InitialSessionState source)
        {
            clean.Variables.Add(source.Variables.Clone());
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
                    throw (new ArgumentException(msg));
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
