# Referencing the .NET SDK
Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Variables such as my TRIM Dataset details
$wgs = "local"
$dbId = "45"

# Path to your tab-delimited input file (no header row - RecordNumber<TAB>CloseDate)
$inputFile = "C:\Temp\RecordsToClose.txt"

# Single log file for this run, named with date and time
$logFileName = "C:\Temp\CloseRecordLog_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

if (-not (Test-Path $inputFile)) {
    Write-Host "Error: Input file not found at $inputFile"
    exit
}

# Connect to the Database
$db = New-Object TRIM.SDK.Database
$db.WorkgroupServerName = $wgs
$db.Id = $dbId
$db.Connect()

# Read the tab-delimited file
$rows = Import-Csv -Path $inputFile -Delimiter "`t" -Header "RecordNumber","CloseDate"

# Collect log lines in memory - write once at the end instead of per-record file I/O
$logLines = New-Object System.Collections.Generic.List[string]

foreach ($row in $rows)
{
  $recordNumber = $row.RecordNumber.Trim()
  $closeDateRaw = $row.CloseDate.Trim()

  # Parse DD/MM/YYYY into a .NET DateTime
  try {
    $parsedDate = [DateTime]::ParseExact($closeDateRaw, "dd/MM/yyyy", $null)
  }
  catch {
    $logLines.Add("$recordNumber failed to set closed date")
    continue
  }

  # Search for the record
  $recordSearch = New-Object TRIM.SDK.TrimMainObjectSearch($db, [TRIM.SDK.BaseObjectTypes]::Record)
  $recordSearch.SetSearchString("number:$recordNumber")

  $found = $false

  foreach ($record in $recordSearch)
  {
    $found = $true

    $closeOptions = New-Object TRIM.SDK.CloseRecordOptions
    $closeOptions.SpecificCloseDate = New-Object TRIM.SDK.TrimDateTime($parsedDate)

    $success = $record.CloseRecord($closeOptions)

    if ($success) {
      $logLines.Add("$($record.Number) closed date set successfully")
    } else {
      $logLines.Add("$($record.Number) failed to set closed date")
    }
  }

  if (-not $found) {
    $logLines.Add("$recordNumber failed to set closed date")
  }
}

# Write all log lines to disk in a single operation
[System.IO.File]::WriteAllLines($logFileName, $logLines)

Write-Host "Processed $($rows.Count) record(s)."
Write-Host "Log file: $logFileName"