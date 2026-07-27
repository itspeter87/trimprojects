# Referencing the .NET SDK
Add-Type -Path "C:\Program Files\Micro Focus\Content Manager\TRIM.SDK.dll"

# Construct a new TRIM Enterprise Studio configuration object
$trimConfig = New-Object TRIM.SDK.TrimEnterpriseConfiguration

# Configuring a new dataset of type SQL
$newDataset = $trimConfig.NewDataset([TRIM.SDK.DatabaseTypes]::SqlServer,'TD','trimdataset')
$newDataset.SetConnectionString('Database=trimdatabase;Driver=ODBC Driver 17 for SQL Server;Encrypt=Yes;Server=trimbox;Trusted_Connection=Yes;TrustServerCertificate=Yes')
$newDataset.SetConnectionPassword('mypassword')

# Modify an existing dataset using the dataset ID
$exDataset = $trimConfig.FindDataset('TT')

# Adding a new Workgroup Server and enabling event processing
$newWGS = $trimConfig.NewWorkgroupServer('trimbox','1137')
$newWGS.CanProcessEvents = 0

# Modiying an exisitng Workgroup Server and enabling Open ID authentication - This is also handy for updating certificates on your https connections  
$exWGS = $trimConfig.FindWorkgroupServer('001')
$exWGS.SetAuthenticationMethod([TRIM.SDK.AuthenticationMethod]::OpenId, 1)

# Remember to always save your configuration changes 
$trimConfig.Save()

# Deploy your configuration
$trimConfig.Deploy()