function Get-NetworkAdapter
{
    Param (
        [Parameter(mandatory=$true)]
        [String]
        $ComputerName
    )

    Process {
        write-verbose "Getting network adapters from $ComputerName"

        $nic = Get-WmiObject -ComputerName $ComputerName -Class Win32_NetworkAdapter

        if( $null -ne $nic ) {
            $nic
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
        $routes = Invoke-Command -ComputerName $ComputerName -ScriptBlock {
            Get-Item "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\PersistentRoutes" | 
				select -ExpandProperty Property
        }

        $routes
    }
}
$VerbosePreference = "Continue"
# Get-PersistentRoutes -ComputerName "vwin8"

