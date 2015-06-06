
Param (
	# [Parameter(mandatory=$true)]
	$ComputerName = "vwin8",

	$Credentials
)

Begin {
	$netmodule_objects = @"
	using System;
	using System.Collections.Generic;


	namespace NetModule {
	public class NetworkAdapter {
		public String Caption = String.Empty;
		public List<PersistentRoute> Routes = new List<PersistentRoute>();
		public List<IPAddress> IPAddresses = new List<IPAddress>();
	}
	
	public class PersistentRoute {
		public String Route = String.Empty;
	}

	public class IPAddress {
		
		public String Address = String.Empty;
		public String SubnetMask = String.Empty;
	}
	
	}
"@

Add-Type -TypeDefinition $netmodule_objects -Language CSharp 

}

Process {
	# returns IP Enabled network adapters.
	function Get-NetworkAdapter
	{
		Param (
			[Parameter(mandatory=$true)]
			[String]
			$ComputerName
		)

		Process {
			write-verbose "Getting network adapters from $ComputerName"

			[NetModule.NetworkAdapter[]]$nic = Get-WmiObject -ComputerName $ComputerName -Class Win32_NetworkAdapter |
				select-Object -ExpandProperty Caption |
				foreach-object {
					new-object NetModule.NetworkAdapter -Property @{
						Caption = $_
					}
				}
			$nic
		}
	}

	function Get-PersistentRoutes
	{
		Param (
			[Parameter(mandatory=$true)]
			[String]
			$ComputerName
		)

		Process {

			write-Verbose "Getting static routes from $ComputerName"

			$reg = [Microsoft.Win32.RegistryKey]::OpenRemoteBaseKey('LocalMachine', $ComputerName)
			$subkey = $reg.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\PersistentRoutes")

			$subkey.GetValueNames() |
			foreach-object {
				$value = $_ 

				new-object NetModule.PersistentRoute -Property  @{
					Route = $value
				}
			}
		}
	}

	function Get-NetworkSettings
	{
		Param(
			[Parameter(mandatory=$true)]
			[String]
			$ComputerName
		)

		Process {

            $all_ips = New-Object System.Collections.ArrayList
			write-Verbose "Getting network adapter settings from $ComputerName"
			$nics = Get-WmiObject -ComputerName $ComputerName `
				-Class Win32_NetworkAdapterConfiguration

			$enabled_nics = $nics | 
			select-object Index,Description,IPAddress,IPSubnet,IPEnabled,DefaultIPGateway |
            where-object { $_.IPEnabled } 
			
			$enabled_nics |
            foreach-object {
                $nic = $_
                $ips = $nic.IPAddress
                $ips | foreach { 

					$all_ips.Add($_)
				}
            }

            $all_ips

		}
	}

	$network_adapters = Get-NetworkAdapter -ComputerName $ComputerName
	Get-PersistentRoutes -ComputerName $ComputerName
	# Get-NetworkSettings -ComputerName $ComputerName 

}

