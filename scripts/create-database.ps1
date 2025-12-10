# Script to create SQL Server test database
# Run as Administrator

$serverName = "(localdb)\mssqllocaldb"
$databaseName = "InventarioLocal"
$dataPath = "C:\Data\Inventory"

# Create data directory if it doesn't exist
if (!(Test-Path $dataPath)) {
    New-Item -ItemType Directory -Path $dataPath -Force | Out-Null
    Write-Host "Diretório criado: $dataPath"
}

# Connection string
$connectionString = "Server=$serverName;Database=master;Integrated Security=true;"

try {
    # Load SQL Server assemblies
    [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | out-null
    
    $server = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $serverName
    
    # Check if database exists
    if ($server.Databases[$databaseName]) {
        Write-Host "Banco de dados já existe: $databaseName"
        Write-Host "Para recriar, execute: DROP DATABASE [$databaseName]"
    } else {
        # Create database
        $db = New-Object ('Microsoft.SqlServer.Management.Smo.Database') $server, $databaseName
        $db.Create()
        
        Write-Host "Banco de dados criado com sucesso: $databaseName"
        
        # Connection string to use in the app
        $connStr = "Server=$serverName;Database=$databaseName;Integrated Security=true;"
        Write-Host ""
        Write-Host "Connection string para o sqlserver.config.json:"
        Write-Host $connStr
    }
    
    # Display connection info
    Write-Host ""
    Write-Host "Informações do servidor:"
    Write-Host "Server: $serverName"
    Write-Host "Database: $databaseName"
    Write-Host ""
    Write-Host "A aplicação criará automaticamente as tabelas ao iniciar."
}
catch {
    Write-Error "Erro ao criar banco de dados: $_"
    Write-Host "Certifique-se de que SQL Server LocalDB está instalado."
    Write-Host "Você pode criar o banco manualmente usando SQL Server Management Studio."
}
