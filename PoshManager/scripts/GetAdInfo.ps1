$adcomputers = Get-ADComputer -Filter *

$enabled = $adcomputers | 
    where-object { $_.Enabled } |
    select-object Name,DNSHostName


$enabled | ForEach-Object {
    $en = $_
	write-verbose "Attempting to ping $($en.DNSHostName)"
    $ping = Test-Connection -ComputerName $en.DNSHostName -Count 1 -ErrorAction SilentlyContinue
    
    $pok = $false
    if( $ping ) {
        $pok = $true
    }

    $en | select-object Name,DNSHostName,@{Name="Ping"; Expression={$pok}}

}

 