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



$locSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::Location)
# Adjust the SearchString as needed. Example: "internal and active and type:person", "internal and ValidLogin", "saved:[the uri of the saved search you made]"
$locSearch.SetSearchString("ValidLogin")
foreach ($loc in $locSearch)

{
    $histSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::History)
    $histSearch.SearchString = "user:$($loc.Uri) and date:Previous 60 Days"

    if ($histSearch.Count -eq 0)
    {

     Write-Host "Disabling location" $loc.FormattedName
     $loc.Deactivate()
     $loc.SetNotes("System has made account inactive due to user inactivity in the last 60 days.", [TRIM.SDK.NotesUpdateType]::PrependWithUserStamp)
     $loc.CanLogin = 0
     $loc.Save();
    }

}



