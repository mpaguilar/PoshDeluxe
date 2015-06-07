
Param (
	# [Parameter(mandatory=$true)]
	$ComputerName = "vwin8",

	$Credentials
)

Begin {
	$netmodule_objects = @"
	using System;
	using System.Collections.Generic;


	namespace NetModules {
	public class NetworkAdapter {
		public Int32 Id = -1;
		public String MACAddress = String.Empty;
		public Int32 MaxSpeed = -1;
		public bool NetEnabled = false;
		public String Description = String.Empty;
		public List<PersistentRoute> Routes = new List<PersistentRoute>();
		public List<IPAddress> IPAddresses = new List<IPAddress>();
	}
	
	public class PersistentRoute {
		public String Route = String.Empty;
	}

	public class IPAddress {
		
		public String Address = String.Empty;
		public String SubnetMask = String.Empty;

		public override String ToString() {
			return String.Format("{0}/{1}", Address, SubnetMask);
		}
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

			Get-WmiObject -Class Win32_NetworkAdapter -ComputerName $ComputerName |
				select-object DeviceID, MACAddress, MaxSpeed, NetEnabled, Description |
				foreach-Object {
					$nic = $_ 
					new-Object NetModules.NetworkAdapter -Property @{
						Id = $nic.DeviceID
						MACAddress = $nic.MACAddress
						MaxSpeed = $nic.MaxSpeed
						NetEnabled = $nic.NetEnabled
						Description = $nic.Description
					}
				}
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

				new-object NetModules.PersistentRoute -Property  @{
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
				-Class Win32_NetworkAdapterConfiguration | 
				select-object Index,Description,IPAddress,IPSubnet,IPEnabled,DefaultIPGateway
			
			$nics
		}
	}

	function Get-CombinedNetSettings 
	{
		Param(
			[Parameter(mandatory=$true)]
			[String]
			$ComputerName
		)

		Process {
			$network_adapters = Get-NetworkAdapter -ComputerName $ComputerName

			$network_settings = Get-NetworkSettings -ComputerName $ComputerName 

			$network_adapters |
			foreach-Object {
				$adp = $_ 
				$set = $network_settings  | where-Object  { $_.Index -eq $adp.Id }

				for( $x = 0; $x -lt $set.IPAddress.Length; $x = $x + 1 )
				{
					$ip = $set.IPAddress[$x]
					$mask = $set.IPSubnet[$x]
					$addy = new-Object NetModules.IPAddress -Property @{
						Address = $ip 
						SubnetMask = $mask
					}
					$adp.IPAddresses.Add($addy);
				}

				$adp
			}
		}
	}

	Get-CombinedNetSettings -ComputerName $ComputerName 

}

