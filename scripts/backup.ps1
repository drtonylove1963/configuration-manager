# Configuration Manager Backup Script
param(
    [string]$BackupPath = "./backups",
    [string]$RetentionDays = "30"
)

# Configuration
$ComposeFile = "docker-compose.prod.yml"
$LogFile = "./logs/backup.log"

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

function Create-Backup {
    Write-Log "Starting backup process..."
    
    $backupTimestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $currentBackupPath = Join-Path $BackupPath "backup_$backupTimestamp"
    
    # Create backup directory
    New-Item -ItemType Directory -Path $currentBackupPath -Force | Out-Null
    Write-Log "Created backup directory: $currentBackupPath"
    
    # Load environment variables
    if (Test-Path ".env.prod") {
        Get-Content ".env.prod" | ForEach-Object {
            if ($_ -match '^([^#][^=]+)=(.*)$') {
                [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
            }
        }
    }
    
    # Backup SQL Server
    $sqlPassword = [Environment]::GetEnvironmentVariable("SQL_SERVER_PASSWORD")
    if (!$sqlPassword) { $sqlPassword = "ConfigManager123!" }
    
    Write-Log "Backing up SQL Server database..."
    try {
        # Create backup inside container
        docker exec configmanager-sqlserver-prod /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $sqlPassword -Q "BACKUP DATABASE [ConfigurationManager] TO DISK = '/tmp/configmanager_backup.bak' WITH FORMAT, INIT"
        
        if ($LASTEXITCODE -eq 0) {
            # Copy backup file from container
            docker cp configmanager-sqlserver-prod:/tmp/configmanager_backup.bak "$currentBackupPath/sqlserver_backup.bak"
            Write-Log "SQL Server backup completed successfully" "SUCCESS"
        }
        else {
            Write-Log "SQL Server backup failed" "ERROR"
        }
    }
    catch {
        Write-Log "SQL Server backup error: $($_.Exception.Message)" "ERROR"
    }
    
    # Backup MongoDB
    $mongoPassword = [Environment]::GetEnvironmentVariable("MONGO_PASSWORD")
    if (!$mongoPassword) { $mongoPassword = "ConfigManager123!" }
    
    Write-Log "Backing up MongoDB database..."
    try {
        # Create MongoDB backup
        docker exec configmanager-mongo-prod mongodump --username admin --password $mongoPassword --authenticationDatabase admin --out /tmp/mongo_backup
        
        if ($LASTEXITCODE -eq 0) {
            # Copy backup files from container
            docker cp configmanager-mongo-prod:/tmp/mongo_backup "$currentBackupPath/mongodb_backup"
            Write-Log "MongoDB backup completed successfully" "SUCCESS"
        }
        else {
            Write-Log "MongoDB backup failed" "ERROR"
        }
    }
    catch {
        Write-Log "MongoDB backup error: $($_.Exception.Message)" "ERROR"
    }
    
    # Backup configuration files
    Write-Log "Backing up configuration files..."
    try {
        $configBackupPath = Join-Path $currentBackupPath "config"
        New-Item -ItemType Directory -Path $configBackupPath -Force | Out-Null
        
        # Copy important configuration files
        $configFiles = @(
            "docker-compose.prod.yml",
            ".env.prod",
            "config/nginx.conf",
            "config/prometheus.yml"
        )
        
        foreach ($file in $configFiles) {
            if (Test-Path $file) {
                $fileName = Split-Path $file -Leaf
                Copy-Item $file (Join-Path $configBackupPath $fileName)
            }
        }
        
        Write-Log "Configuration files backup completed" "SUCCESS"
    }
    catch {
        Write-Log "Configuration backup error: $($_.Exception.Message)" "ERROR"
    }
    
    # Create backup metadata
    $metadata = @{
        BackupDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        BackupType = "Full"
        Version = "1.0"
        Services = @("SQL Server", "MongoDB", "Configuration Files")
    }
    
    $metadata | ConvertTo-Json | Out-File (Join-Path $currentBackupPath "backup_metadata.json")
    
    # Compress backup
    Write-Log "Compressing backup..."
    try {
        $zipPath = "$currentBackupPath.zip"
        Compress-Archive -Path $currentBackupPath -DestinationPath $zipPath -Force
        Remove-Item -Path $currentBackupPath -Recurse -Force
        Write-Log "Backup compressed to: $zipPath" "SUCCESS"
    }
    catch {
        Write-Log "Compression error: $($_.Exception.Message)" "WARNING"
    }
    
    Write-Log "Backup process completed" "SUCCESS"
    return $zipPath
}

function Remove-OldBackups {
    param([string]$Path, [int]$RetentionDays)
    
    Write-Log "Cleaning up old backups (retention: $RetentionDays days)..."
    
    try {
        $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
        $oldBackups = Get-ChildItem -Path $Path -Filter "backup_*.zip" | Where-Object { $_.CreationTime -lt $cutoffDate }
        
        foreach ($backup in $oldBackups) {
            Remove-Item $backup.FullName -Force
            Write-Log "Removed old backup: $($backup.Name)"
        }
        
        Write-Log "Cleanup completed. Removed $($oldBackups.Count) old backups" "SUCCESS"
    }
    catch {
        Write-Log "Cleanup error: $($_.Exception.Message)" "ERROR"
    }
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
Write-Log "Configuration Manager Backup Script Started"

# Ensure backup directory exists
if (!(Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
}

# Check if services are running
if (!(Test-Services)) {
    Write-Log "Some services are not running. Backup may be incomplete." "WARNING"
}

# Create backup
$backupFile = Create-Backup

# Clean up old backups
Remove-OldBackups -Path $BackupPath -RetentionDays $RetentionDays

Write-Log "Backup script completed. Backup saved to: $backupFile" "SUCCESS"
