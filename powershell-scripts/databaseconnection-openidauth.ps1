# Referencing the .NET SDK
Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Variables such as my TRIM Dataset details
$wgs = "local"
$dbId = "45"

# Connect to the Database
$db = New-Object TRIM.SDK.Database

# Tell it to connect via Open ID Authentication and that the app has a user interface to enter the credentials, for example: Microsoft Entra login details
$db.AuthenticationMethod = [TRIM.SDK.ClientAuthenticationMechanism]::OpenId
[TRIM.SDK.TrimApplication]::HasUserInterface = $true
#NOTE: If connecting via https use $db.WorkgroupServerURL rather than $db.WorkgroupServerName
$db.WorkgroupServerName = $wgs
$db.Id = $dbId
$db.Connect()

Write-Host $db.Name