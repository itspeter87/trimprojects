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

# Update the network login

$locSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::Location)

# Adjust the SearchString as needed. Example: "internal and active and type:person", "internal and ValidLogin", "saved:[the uri of the saved search you made]"
$locSearch.SetSearchString("saved:5")

$loginUpdateMethod = Read-Host "Choose login method: 1 = Use existing email, 2 = Firstname.Lastname, 3 = Custom"
$loginFieldChoice  = Read-Host "Choose login field: 1 = LogsInAs, 2 = AdditionalLogin, 3 = SecondAdditionalLogin"

# Only ask for domain if the method needs it
$emailDomain = $null
if ($loginUpdateMethod -eq 2) {
    $emailDomain = Read-Host "Enter email domain (e.g. myorganisation.com)"
}

foreach ($loc in $locSearch)
{
    # -----------------------------
    # Determine login value
    # -----------------------------
    $newLogin = switch ($loginUpdateMethod)
    {
        1 {
            # Method 1: Use existing email address
            $loc.InternetMailAddress
        }

        2 {
            # Method 2: firstname.lastname@domain
            ($loc.GivenNames + "." + $loc.Surname + "@" + $emailDomain)
        }

        3 {
            # Method 3: Custom logic placeholder
            "#mycodehere"
        }

        default {
            Write-Host "Invalid login update method."
            continue
        }
    }

    # -----------------------------
    # Apply login value to chosen field
    # -----------------------------
    switch ($loginFieldChoice)
    {
        1 { $loc.LogsInAs              = $newLogin }
        2 { $loc.AdditionalLogin       = $newLogin }
        3 { $loc.SecondAdditionalLogin = $newLogin }

        default {
            Write-Host "Invalid login field choice."
            continue
        }
    }

    # Save once per location
    $loc.Save()

    Write-Host "Updated $($loc.Name) -> $newLogin"
}




