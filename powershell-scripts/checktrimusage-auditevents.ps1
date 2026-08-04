# Referencing the .NET SDK
Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Prompt the user for authentication type FIRST
Write-Host "Choose authentication method:"
Write-Host "1 = Windows Authentication"
Write-Host "2 = OpenID Authentication"
Write-Host "3 = Use default client dataset settings"
$choice = Read-Host "Enter 1, 2 or 3"

# Create the database object
$db = New-Object TRIM.SDK.Database

switch ($choice) {

    "1" {
        Write-Host "Using Windows Authentication..."

        $workgroupServer = Read-Host "Enter Workgroup Server Name (e.g., local)"
        $datasetId = Read-Host "Enter Dataset ID"

        $db.WorkgroupServerName = $workgroupServer
        $db.Id = $datasetId
    }

    "2" {
        Write-Host "Using OpenID Authentication..."

        $workgroupServer = Read-Host "Enter Workgroup Server URL (e.g., https://server.domain)"
        $datasetId = Read-Host "Enter Dataset ID"

        $db.AuthenticationMethod = [TRIM.SDK.ClientAuthenticationMechanism]::OpenId
        [TRIM.SDK.TrimApplication]::HasUserInterface = $true

        $db.WorkgroupServerURL = $workgroupServer
        $db.Id = $datasetId
    }

    "3" {
        Write-Host "Using default dataset configuration..."
    }

    default {
        Write-Host "Invalid selection. Exiting."
        exit
    }
}

# Connect to the dataset
$db.Connect()

Write-Host "Connected to dataset:" $db.Name

# ============================
#  TXT OUTPUT FILE SETUP
# ============================

$folderPath = Read-Host "Enter folder path to save the audit file (e.g. C:\temp)"

if (!(Test-Path $folderPath)) {
    Write-Host "Folder does not exist. Creating it..."
    New-Item -ItemType Directory -Path $folderPath | Out-Null
}

$timestamp = (Get-Date).ToString("dd-MM-yyyy-HH-mm-ss")
$fileName = "trim-user_audit_check_$timestamp.txt"
$outputFile = Join-Path $folderPath $fileName

Write-Host "Saving results to: $outputFile"

# Write tab‑delimited headings
"Location`tHistory Events" | Out-File -FilePath $outputFile -Append

# ============================
#  LOCATION SEARCH
# ============================

$locSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::Location)
$locSearch.SetSearchString("saved:18")
foreach ($loc in $locSearch)
{
    $locationLabel = "$($loc.FormattedName) ($($loc.Uri))"

    $events = @()

    $histSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::History)
    $histSearch.SearchString = "user:$($loc.Uri)"
    $histSearch.LimitOnRowsReturned = 10

    foreach ($hist in $histSearch)
    {
        $events += $hist.EventDescription
    }

    # Pipe-delimited history events
    $eventString = ($events -join " | ")

    $line = "$locationLabel`t$eventString"

    Write-Host $line
    $line | Out-File -FilePath $outputFile -Append
}
