

Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Variables such as my TRIM Dataset details
$wgs = "local"
$dbId = "45"

# Connect to the Database

$db = New-Object TRIM.SDK.Database
$db.WorkgroupServerName = $wgs
$db.Id = $dbId
$db.Connect()

# Update the network login

$locSearch = New-Object TRIM.SDK.TrimMainObjectSearch
$locSearch.SetSearchString()

foreach($location in $locSearch)
{
  # Get the SMTP email address

  

}



