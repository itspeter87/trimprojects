# Referencing the .NET SDK
Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Variables such as my TRIM Dataset details
$wgs = "local"
$dbId = "45"

# Connect to the Database

$db = New-Object TRIM.SDK.Database
$db.WorkgroupServerName = $wgs
$db.Id = $dbId
$db.Connect()

# Search for all unknown locations
$locSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::Location)
$locSearch.SetSearchString("type:unknown")

foreach ($loc in $locSearch)
{

# Search if unknown location has any associated records
 $recSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::Record) 
 $recSearch.SetSearchString("contactx:[location:$($loc.Uri)]")

 if ($recSearch.Count -gt 0)
 {
   Write-Host "$($loc.SortName) has contact records." -NoNewline
   Write-Host " Skipping location" -ForegroundColor Green
 }

 else
 {
   Write-Host "$($loc.SortName) has no contact records." -NoNewline
   Write-Host " Deleting location" -ForegroundColor Red
   $loc.Delete()
 }

}