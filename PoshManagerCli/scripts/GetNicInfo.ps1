
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
			[String[]]$routes = Invoke-Command -ComputerName $ComputerName -ScriptBlock {
				Get-Item "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\PersistentRoutes" | 
					select-object -ExpandProperty Property
			}

			$routes
		}
	}


	$Global:NetworkAdapters = Get-NetworkAdapter -ComputerName $ComputerName
	$Global:PersistentRoutes = Get-PersistentRoutes -ComputerName $ComputerName 
}

