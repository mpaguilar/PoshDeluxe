
Param (
	[Parameter(mandatory=$true)]
	$ComputerName,

	$Credentials
)

Process {
	function Get-NetworkAdapter
	{
		Param (
			[Parameter(mandatory=$true)]
			[String]
			$ComputerName
		)

		Process {
			write-verbose "Getting network adapters from $ComputerName"

			[String[]]$nic = Get-WmiObject -ComputerName $ComputerName -Class Win32_NetworkAdapter |
				select-Object -ExpandProperty Caption

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
			[String[]]$routes = @(,$subkey.GetValueNames())

			$routes

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

			$nics | select Index,Description,IPAddress,IPSubnet,IPEnabled,DefaultIPGateway |
            where { $_.IPEnabled } |
            foreach {
                $nic = $_
                $ips = $nic.IPAddress
                $ips | foreach { $all_ips.Add($_) | out-null }
            }

            $all_ips

		}
	}

	$Global:NetworkAdapters = Get-NetworkAdapter -ComputerName $ComputerName
	$Global:PersistentRoutes = Get-PersistentRoutes -ComputerName $ComputerName
	Get-NetworkSettings -ComputerName $ComputerName 

}

