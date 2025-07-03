# Configuration Manager Production Deployment Script
param(
    [Parameter(Position=0)]
    [ValidateSet("deploy", "backup", "status", "logs", "stop", "restart")]
    [string]$Action = "deploy",
    
    [Parameter(Position=1)]
    [string]$ServiceName = ""
)

# Configuration
$ComposeFile = "docker-compose.prod.yml"
$EnvFile = ".env.prod"
$BackupDir = "./backups"
$LogFile = "./logs/deploy.log"

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

function Test-Prerequisites {
    Write-Log "Checking prerequisites..."
    
    # Check Docker
    try {
        docker --version | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Docker command failed" }
    }
    catch {
        Write-Log "Docker is not installed or not accessible" "ERROR"
        exit 1
    }
    
    # Check Docker Compose
    try {
        docker-compose --version | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Docker Compose command failed" }
    }
    catch {
        Write-Log "Docker Compose is not installed or not accessible" "ERROR"
        exit 1
    }
    
    # Check Compose file
    if (!(Test-Path $ComposeFile)) {
        Write-Log "Docker Compose file not found: $ComposeFile" "ERROR"
        exit 1
    }
    
    Write-Log "Prerequisites check passed" "SUCCESS"
}

function Initialize-Directories {
    Write-Log "Creating necessary directories..."
    
    $directories = @(
        "logs",
        "backups", 
        "config/ssl",
        "config/grafana/provisioning"
    )
    
    foreach ($dir in $directories) {
        if (!(Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }
    }
    
    Write-Log "Directories created" "SUCCESS"
}

function Import-Environment {
    Write-Log "Loading environment variables..."
    
    if (Test-Path $EnvFile) {
        Get-Content $EnvFile | ForEach-Object {
            if ($_ -match '^([^#][^=]+)=(.*)$') {
                [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
            }
        }
        Write-Log "Environment variables loaded from $EnvFile" "SUCCESS"
    }
    else {
        Write-Log "Environment file not found: $EnvFile. Using defaults." "WARNING"
    }
}

function Backup-Data {
    Write-Log "Creating backup..."
    
    $backupTimestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupPath = Join-Path $BackupDir "backup_$backupTimestamp"
    
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    
    # Backup SQL Server data if container exists
    $sqlContainer = docker ps -a --format "table {{.Names}}" | Select-String "configmanager-sqlserver-prod"
    if ($sqlContainer) {
        Write-Log "Backing up SQL Server data..."
        $sqlPassword = [Environment]::GetEnvironmentVariable("SQL_SERVER_PASSWORD")
        if (!$sqlPassword) { $sqlPassword = "ConfigManager123!" }
        
        docker exec configmanager-sqlserver-prod /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $sqlPassword -Q "BACKUP DATABASE [ConfigurationManager] TO DISK = '/tmp/configmanager_backup.bak'"
        if ($LASTEXITCODE -eq 0) {
            docker cp configmanager-sqlserver-prod:/tmp/configmanager_backup.bak "$backupPath/"
        }
        else {
            Write-Log "SQL Server backup failed" "WARNING"
        }
    }
    
    # Backup MongoDB data if container exists
    $mongoContainer = docker ps -a --format "table {{.Names}}" | Select-String "configmanager-mongo-prod"
    if ($mongoContainer) {
        Write-Log "Backing up MongoDB data..."
        docker exec configmanager-mongo-prod mongodump --out /tmp/mongo_backup
        if ($LASTEXITCODE -eq 0) {
            docker cp configmanager-mongo-prod:/tmp/mongo_backup "$backupPath/"
        }
        else {
            Write-Log "MongoDB backup failed" "WARNING"
        }
    }
    
    Write-Log "Backup created at $backupPath" "SUCCESS"
}

function Start-Deployment {
    Write-Log "Starting deployment..."
    
    # Pull latest images
    Write-Log "Pulling latest images..."
    docker-compose -f $ComposeFile pull
    
    # Build application images
    Write-Log "Building application images..."
    docker-compose -f $ComposeFile build --no-cache
    
    # Stop existing services
    Write-Log "Stopping existing services..."
    docker-compose -f $ComposeFile down
    
    # Start services
    Write-Log "Starting services..."
    docker-compose -f $ComposeFile up -d
    
    Write-Log "Deployment completed" "SUCCESS"
}

function Test-Health {
    Write-Log "Performing health checks..."
    
    # Wait for services to start
    Start-Sleep -Seconds 30
    
    # Check API health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8001/health" -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Log "API health check passed" "SUCCESS"
        }
        else {
            throw "API returned status code: $($response.StatusCode)"
        }
    }
    catch {
        Write-Log "API health check failed: $($_.Exception.Message)" "ERROR"
        exit 1
    }
    
    # Check Blazor app
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8002" -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Log "Blazor app health check passed" "SUCCESS"
        }
        else {
            throw "Blazor app returned status code: $($response.StatusCode)"
        }
    }
    catch {
        Write-Log "Blazor app health check failed: $($_.Exception.Message)" "ERROR"
        exit 1
    }
    
    # Check Nginx
    try {
        $response = Invoke-WebRequest -Uri "http://localhost/health" -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Log "Nginx health check passed" "SUCCESS"
        }
        else {
            throw "Nginx returned status code: $($response.StatusCode)"
        }
    }
    catch {
        Write-Log "Nginx health check failed: $($_.Exception.Message)" "ERROR"
        exit 1
    }
    
    Write-Log "All health checks passed" "SUCCESS"
}

function Show-Status {
    Write-Log "Deployment status:"
    docker-compose -f $ComposeFile ps
    
    Write-Host ""
    Write-Log "Service URLs:"
    Write-Host "  - Application: http://localhost" -ForegroundColor Cyan
    Write-Host "  - API: http://localhost:8001" -ForegroundColor Cyan
    Write-Host "  - Blazor App: http://localhost:8002" -ForegroundColor Cyan
    Write-Host "  - Grafana: http://localhost:3000" -ForegroundColor Cyan
    Write-Host "  - Prometheus: http://localhost:9090" -ForegroundColor Cyan
    Write-Host "  - RabbitMQ Management: http://localhost:15672" -ForegroundColor Cyan
}

function Invoke-MainDeployment {
    Write-Log "Starting Configuration Manager deployment..."
    
    Initialize-Directories
    Test-Prerequisites
    Import-Environment
    
    # Ask for confirmation in production
    $environment = [Environment]::GetEnvironmentVariable("ENVIRONMENT")
    if ($environment -eq "production") {
        Write-Host "WARNING: You are deploying to PRODUCTION environment!" -ForegroundColor Yellow
        $confirmation = Read-Host "Are you sure you want to continue? (y/N)"
        if ($confirmation -ne "y" -and $confirmation -ne "Y") {
            Write-Log "Deployment cancelled by user"
            exit 0
        }
        
        Backup-Data
    }
    
    Start-Deployment
    Test-Health
    Show-Status
    
    Write-Log "Configuration Manager deployed successfully!" "SUCCESS"
}

# Main execution
switch ($Action) {
    "deploy" {
        Invoke-MainDeployment
    }
    "backup" {
        Initialize-Directories
        Import-Environment
        Backup-Data
    }
    "status" {
        Show-Status
    }
    "logs" {
        if ($ServiceName) {
            docker-compose -f $ComposeFile logs -f $ServiceName
        }
        else {
            docker-compose -f $ComposeFile logs -f
        }
    }
    "stop" {
        Write-Log "Stopping services..."
        docker-compose -f $ComposeFile down
        Write-Log "Services stopped" "SUCCESS"
    }
    "restart" {
        Write-Log "Restarting services..."
        if ($ServiceName) {
            docker-compose -f $ComposeFile restart $ServiceName
        }
        else {
            docker-compose -f $ComposeFile restart
        }
        Write-Log "Services restarted" "SUCCESS"
    }
    default {
        Write-Host "Usage: .\deploy.ps1 {deploy|backup|status|logs|stop|restart} [service-name]"
        Write-Host "  deploy  - Deploy the application (default)"
        Write-Host "  backup  - Create a backup of data"
        Write-Host "  status  - Show service status"
        Write-Host "  logs    - Show service logs (optionally specify service name)"
        Write-Host "  stop    - Stop all services"
        Write-Host "  restart - Restart services (optionally specify service name)"
        exit 1
    }
}
