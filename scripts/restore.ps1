# Configuration Manager Restore Script
param(
    [Parameter(Mandatory=$true)]
    [string]$BackupFile,
    
    [switch]$Force
)

# Configuration
$ComposeFile = "docker-compose.prod.yml"
$LogFile = "./logs/restore.log"
$TempDir = "./temp_restore"

# Functions
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    switch ($Level) {
        "ERROR" { Write-Host $logMessage -ForegroundColor Red }
        "WARNING" { Write-Host $logMessage -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $logMessage -ForegroundColor Green }
        default { Write-Host $logMessage -ForegroundColor Blue }
    }
    
    # Ensure log directory exists
    $logDir = Split-Path $LogFile -Parent
    if (!(Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    
    Add-Content -Path $LogFile -Value $logMessage
}

function Test-BackupFile {
    param([string]$FilePath)
    
    if (!(Test-Path $FilePath)) {
        Write-Log "Backup file not found: $FilePath" "ERROR"
        return $false
    }
    
    if ($FilePath -notlike "*.zip") {
        Write-Log "Backup file must be a ZIP archive" "ERROR"
        return $false
    }
    
    return $true
}

function Extract-Backup {
    param([string]$BackupFile, [string]$ExtractPath)
    
    Write-Log "Extracting backup file..."
    
    try {
        if (Test-Path $ExtractPath) {
            Remove-Item -Path $ExtractPath -Recurse -Force
        }
        
        Expand-Archive -Path $BackupFile -DestinationPath $ExtractPath -Force
        Write-Log "Backup extracted successfully" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "Failed to extract backup: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Get-BackupMetadata {
    param([string]$ExtractPath)
    
    $metadataFile = Join-Path $ExtractPath "backup_metadata.json"
    
    if (Test-Path $metadataFile) {
        try {
            $metadata = Get-Content $metadataFile | ConvertFrom-Json
            Write-Log "Backup metadata loaded:"
            Write-Log "  Date: $($metadata.BackupDate)"
            Write-Log "  Type: $($metadata.BackupType)"
            Write-Log "  Version: $($metadata.Version)"
            return $metadata
        }
        catch {
            Write-Log "Failed to read backup metadata" "WARNING"
        }
    }
    else {
        Write-Log "Backup metadata not found" "WARNING"
    }
    
    return $null
}

function Restore-SqlServer {
    param([string]$BackupPath)
    
    $sqlBackupFile = Join-Path $BackupPath "sqlserver_backup.bak"
    
    if (!(Test-Path $sqlBackupFile)) {
        Write-Log "SQL Server backup file not found" "WARNING"
        return $false
    }
    
    Write-Log "Restoring SQL Server database..."
    
    # Load environment variables
    if (Test-Path ".env.prod") {
        Get-Content ".env.prod" | ForEach-Object {
            if ($_ -match '^([^#][^=]+)=(.*)$') {
                [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
            }
        }
    }
    
    $sqlPassword = [Environment]::GetEnvironmentVariable("SQL_SERVER_PASSWORD")
    if (!$sqlPassword) { $sqlPassword = "ConfigManager123!" }
    
    try {
        # Copy backup file to container
        docker cp $sqlBackupFile configmanager-sqlserver-prod:/tmp/restore_backup.bak
        
        # Restore database
        docker exec configmanager-sqlserver-prod /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $sqlPassword -Q "RESTORE DATABASE [ConfigurationManager] FROM DISK = '/tmp/restore_backup.bak' WITH REPLACE"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Log "SQL Server database restored successfully" "SUCCESS"
            return $true
        }
        else {
            Write-Log "SQL Server restore failed" "ERROR"
            return $false
        }
    }
    catch {
        Write-Log "SQL Server restore error: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Restore-MongoDB {
    param([string]$BackupPath)
    
    $mongoBackupPath = Join-Path $BackupPath "mongodb_backup"
    
    if (!(Test-Path $mongoBackupPath)) {
        Write-Log "MongoDB backup directory not found" "WARNING"
        return $false
    }
    
    Write-Log "Restoring MongoDB database..."
    
    $mongoPassword = [Environment]::GetEnvironmentVariable("MONGO_PASSWORD")
    if (!$mongoPassword) { $mongoPassword = "ConfigManager123!" }
    
    try {
        # Copy backup directory to container
        docker cp $mongoBackupPath configmanager-mongo-prod:/tmp/mongo_restore
        
        # Restore database
        docker exec configmanager-mongo-prod mongorestore --username admin --password $mongoPassword --authenticationDatabase admin --drop /tmp/mongo_restore
        
        if ($LASTEXITCODE -eq 0) {
            Write-Log "MongoDB database restored successfully" "SUCCESS"
            return $true
        }
        else {
            Write-Log "MongoDB restore failed" "ERROR"
            return $false
        }
    }
    catch {
        Write-Log "MongoDB restore error: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Confirm-Restore {
    if ($Force) {
        return $true
    }
    
    Write-Host ""
    Write-Host "WARNING: This will restore data from the backup and OVERWRITE existing data!" -ForegroundColor Red
    Write-Host "This action cannot be undone." -ForegroundColor Red
    Write-Host ""
    
    $confirmation = Read-Host "Are you sure you want to continue? Type 'RESTORE' to confirm"
    
    return ($confirmation -eq "RESTORE")
}

function Test-Services {
    Write-Log "Checking service status..."
    
    $services = @("configmanager-sqlserver-prod", "configmanager-mongo-prod")
    $allRunning = $true
    
    foreach ($service in $services) {
        $status = docker ps --filter "name=$service" --format "{{.Status}}"
        if ($status -like "*Up*") {
            Write-Log "$service is running" "SUCCESS"
        }
        else {
            Write-Log "$service is not running" "ERROR"
            $allRunning = $false
        }
    }
    
    return $allRunning
}

# Main execution
Write-Log "Configuration Manager Restore Script Started"

# Validate backup file
if (!(Test-BackupFile -FilePath $BackupFile)) {
    exit 1
}

# Check if services are running
if (!(Test-Services)) {
    Write-Log "Required services are not running. Please start the services first." "ERROR"
    exit 1
}

# Confirm restore operation
if (!(Confirm-Restore)) {
    Write-Log "Restore operation cancelled by user"
    exit 0
}

# Extract backup
if (!(Extract-Backup -BackupFile $BackupFile -ExtractPath $TempDir)) {
    exit 1
}

# Get backup metadata
$metadata = Get-BackupMetadata -ExtractPath $TempDir

# Find the actual backup directory (it might be nested)
$backupDirs = Get-ChildItem -Path $TempDir -Directory | Where-Object { $_.Name -like "backup_*" }
if ($backupDirs.Count -eq 1) {
    $actualBackupPath = $backupDirs[0].FullName
}
else {
    $actualBackupPath = $TempDir
}

Write-Log "Starting restore process..."

# Restore SQL Server
$sqlSuccess = Restore-SqlServer -BackupPath $actualBackupPath

# Restore MongoDB
$mongoSuccess = Restore-MongoDB -BackupPath $actualBackupPath

# Cleanup
Write-Log "Cleaning up temporary files..."
if (Test-Path $TempDir) {
    Remove-Item -Path $TempDir -Recurse -Force
}

# Summary
Write-Log "Restore process completed"
if ($sqlSuccess -and $mongoSuccess) {
    Write-Log "All databases restored successfully" "SUCCESS"
}
elseif ($sqlSuccess -or $mongoSuccess) {
    Write-Log "Partial restore completed. Some databases may have failed." "WARNING"
}
else {
    Write-Log "Restore failed for all databases" "ERROR"
    exit 1
}

Write-Log "Please restart the application services to ensure proper operation"
Write-Log "Use: docker-compose -f $ComposeFile restart api blazorapp"
