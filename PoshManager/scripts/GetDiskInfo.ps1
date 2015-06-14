Param (
	[Parameter(mandatory=$true)]
	$ComputerName,

	$Credentials
)

Begin {

Add-Type -Path "C:\users\michael\Documents\Visual Studio 2013\Projects\RxPlayground\PoshManagerCli\bin\debug\PoshManager.dll"

}

Process {


    function Get-PhysicalDisks {
	    Param (
		    $ComputerName 
	    )

	    Process {
            Get-WmiObject -Class Win32_DiskDrive `
                -Impersonation Impersonate `
                -ComputerName $ComputerName
	    }
    }

    function Get-Disks {
        Param (
            $ComputerName 
        )

        Process {
			write-Verbose "Getting disks from $ComputerName"
            Get-WmiObject -Class Win32_LogicalDisk `
                -Impersonation Impersonate `
                -ComputerName $ComputerName        
        }
    }

    function Get-Partitions {
        Param (
            $ComputerName
        )

        Process {
            Get-WmiObject -Class Win32_DiskPartition `
                -Impersonation Impersonate `
                -ComputerName $ComputerName
        }
    }

	$VerbosePreference = "Continue"
	$DebugPreference = "Continue"

    # Get-PhysicalDisks -ComputerName $ComputerName | 
    #    Select-Object Caption, Name, Model, MediaType, Index, DeviceID, Description, Size
	write-Verbose "Getting disk information from $ComputerName"
    Get-Disks -ComputerName $ComputerName |
	foreach-Object {
		new-Object PoshManager.DiskModule+DiskDrive -Property  @{
			DeviceID = $_.DeviceID
			FileSystem = $_.FileSystem
			Size = $_.Size
			FreeSpace = $_.FreeSpace
			MediaType = $_.MediaType
			Caption = $_.Caption
			Name = $_.Name
		}
	}
        
}