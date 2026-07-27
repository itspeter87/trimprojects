# Referencing the .NET SDK
Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Construct a new TRIM Enterprise Studio configuration object
$trimConfig = New-Object TRIM.SDK.TrimEnterpriseConfiguration

# Get all datasets configured in Enterprise Studio and perform a schema upgrade - Extra powershell parameters needed to stop TRIM Enterprise Studio loading after each command is called per dataset
$datasetCount = $trimConfig.DatasetCount

for ($i = 0; $i -lt $datasetCount; $i++) {

    $ds = $trimConfig.GetDataset([uint32]$i)

    Write-Host "Upgrading dataset:" $ds.DatasetID

    Start-Process "C:\Program Files\Micro Focus\Content Manager\TRIMEnterpriseStudio.exe" -ArgumentList "-d $($ds.DatasetID) -s -r" -WindowStyle Hidden -Wait

}
