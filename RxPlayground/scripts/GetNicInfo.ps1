function Get-NetworkAdapter
{
    Param (
        [Parameter(mandatory=$true)]
        [String]
        $ComputerName
    )

    Process {

        $nic = Get-WmiObject -ComputerName $ComputerName -Class Win32_NetworkAdapter

        if( $null -ne $nic ) {
            $nic
        }

    }
}
$VerbosePreference = "Continue"
write-verbose "GetNicInfo.ps1 included"
write-Warning "This is a warning!"

# Get-NetworkAdapter -ComputerName "vwin8"