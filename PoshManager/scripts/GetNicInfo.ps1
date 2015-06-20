
Param (
	# [Parameter(mandatory=$true)]
	$ComputerName = "vwin8",

	$Credentials
)

Begin {

Add-Type -Path "C:\users\michael\Documents\Visual Studio 2013\Projects\RxPlayground\PoshManagerCli\bin\debug\PoshManager.dll"

}

Process {
	# returns PoshManager.NetModule+NetworkAdapter
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
					new-Object PoshManager.NetModule+NetworkAdapter -Property @{
						Id = $nic.DeviceID
						MACAddress = $nic.MACAddress
						MaxSpeed = $nic.MaxSpeed
						NetEnabled = $nic.NetEnabled
						Description = $nic.Description
					}
				}
			write-verbose "Completed getting network adapters from $ComputerName"
		}
	}

	# returns PoshManager.NetModule+PersistentRoute
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

				new-object PoshManager.NetModule+PersistentRoute -Property  @{
					Route = $value
				}
			}
		}
	}

	# returns raw WMI, is used by NetModule+IPAddress
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
			write-Verbose "Completed getting network adapter settings from $ComputerName"
			$nics
		}
	}

	# returns PoshManager.NetModule+IPAddress
	function Get-CombinedNetSettings 
	{
		Param(
			[Parameter(mandatory=$true)]
			[String]
			$ComputerName
		)

		Process {
			write-verbose "Getting network settings for $ComputerName"
			$network_adapters = Get-NetworkAdapter -ComputerName $ComputerName

			$network_settings = Get-NetworkSettings -ComputerName $ComputerName 

			$network_adapters |
			foreach-Object {
				$adp = $_ 
				write-debug "Looking up $($adp.Description)"
				$set = $network_settings  | where-Object  { $_.Index -eq $adp.Id }

				for( $x = 0; $x -lt $set.IPAddress.Length; $x = $x + 1 )
				{
					$ip = $set.IPAddress[$x]
					$mask = $set.IPSubnet[$x]
					$addy = new-Object PoshManager.NetModule+IPAddress -Property @{
						Address = $ip 
						SubnetMask = $mask
					}
					$adp.IPAddresses.Add($addy);
					write-Debug "Found $($addy)"
				}

				$adp
			}
			write-Verbose "Completed getting settings for $ComputerName"
		}
	}

	$VerbosePreference = "Continue"
	$DebugPreference = "Continue"
	Get-CombinedNetSettings -ComputerName $ComputerName 

}

