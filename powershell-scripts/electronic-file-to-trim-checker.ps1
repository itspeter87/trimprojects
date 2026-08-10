# ============================
#   CONFIGURATION (EDIT THESE)
# ============================

# Prompt user for folder to scan (console input)
do {
    $FolderPath = Read-Host "Enter the directory to scan (e.g. C:\Users or \\server\share)"
    if (-not (Test-Path $FolderPath)) {
        Write-Host "Invalid path. Please try again." -ForegroundColor Red
    }
} until (Test-Path $FolderPath)

# SQL Server connection settings
$SqlServer   = "192.168.0.135"   # Can be on-prem OR Azure SQL
$SqlDatabase = "DemoDB"
$SqlUser     = "sa"

# Prompt for password (console input)
$PlainPwd = Read-Host "Enter SQL Server password"

# Output folder
$OutputFolder = "C:\temp"

# ============================
#   DO NOT EDIT BELOW THIS LINE
# ============================

# Auto-detect Azure SQL vs On-Prem
$IsAzure = $SqlServer.ToLower().EndsWith(".database.windows.net")

if ($IsAzure) {
    Write-Host "Azure SQL detected — applying cloud connection settings." -ForegroundColor Cyan
    $EncryptSetting = "Yes"
    $TrustCertSetting = "No"
} else {
    Write-Host "On-Prem SQL Server detected — applying local connection settings." -ForegroundColor Cyan
    $EncryptSetting = "Yes"
    $TrustCertSetting = "Yes"
}

# Build SQL Server ODBC connection string
$ConnString = "Driver={ODBC Driver 17 for SQL Server};Server=$SqlServer;Database=$SqlDatabase;Uid=$SqlUser;Pwd=$PlainPwd;Encrypt=$EncryptSetting;TrustServerCertificate=$TrustCertSetting;"

# Connect via ODBC
$conn = New-Object System.Data.Odbc.OdbcConnection($ConnString)
$conn.Open()

# -----------------------------
# Load TSRECELEC (hash → uri)
# -----------------------------
$cmdElec = New-Object System.Data.Odbc.OdbcCommand("SELECT uri, reHash FROM TSRECELEC;", $conn)
$readerElec = $cmdElec.ExecuteReader()

$HashToUri = @{}
while ($readerElec.Read()) {
    $HashToUri[$readerElec["reHash"]] = $readerElec["uri"]
}
$readerElec.Close()

# -----------------------------
# Load TSRECORD (uri → recordid)
# -----------------------------
$cmdRec = New-Object System.Data.Odbc.OdbcCommand("SELECT uri, recordid FROM TSRECORD;", $conn)
$readerRec = $cmdRec.ExecuteReader()

$UriToRecordId = @{}
while ($readerRec.Read()) {
    $UriToRecordId[$readerRec["uri"]] = $readerRec["recordid"]
}
$readerRec.Close()

$conn.Close()

# -----------------------------
# Prepare output file
# -----------------------------
$timestamp = (Get-Date).ToString("yyyyMMdd_HHmmss")

if (-not (Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Path $OutputFolder | Out-Null
}

$outFile = "$OutputFolder\integritycheck_$timestamp.txt"

# Write header row (tab-delimited)
"File Path`tRecord Number" | Out-File -FilePath $outFile -Encoding UTF8

Write-Host ""
Write-Host "Starting integrity check..."
Write-Host "Scanning: $FolderPath"
Write-Host "Output file: $outFile"
Write-Host ""

# -----------------------------
# Process filesystem files
# -----------------------------

# Capture all files first (needed for progress bar)
$files = Get-ChildItem -Path $FolderPath -File -Recurse
$total = $files.Count
$counter = 0

foreach ($file in $files) {

    $counter++
    $percent = [int](($counter / $total) * 100)

    Write-Progress -Activity "Integrity Check" `
                   -Status "Processing $counter of $total files" `
                   -PercentComplete $percent

    $hash = Get-FileHash -Path $file.FullName -Algorithm SHA256
    $sha = $hash.Hash

    $result = $null

    # Step 1: Check TSRECELEC (current revision only)
    $uri = $HashToUri[$sha]

    if ($uri) {
        $recordid = $UriToRecordId[$uri]
        if ($recordid) {
            $result = $recordid
        }
    }

    # Step 2: If still not found
    if (-not $result) {
        $result = "No record found"
    }

    # Force string to avoid TypeName prompt
    $line = "$($file.FullName)`t$([string]$result)"

    # Append to file
    $line | Out-File -FilePath $outFile -Append -Encoding UTF8
}

Write-Progress -Activity "Integrity Check" -Completed -Status "Done"

Write-Host "Integrity check complete."
Write-Host "Output saved to: $outFile"
Write-Host ""
