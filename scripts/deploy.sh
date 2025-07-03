#!/bin/bash

# Configuration Manager Production Deployment Script
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="docker-compose.prod.yml"
ENV_FILE=".env.prod"
BACKUP_DIR="./backups"
LOG_FILE="./logs/deploy.log"

# Functions
log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
    exit 1
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "$LOG_FILE"
}

success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1" | tee -a "$LOG_FILE"
}

# Create necessary directories
create_directories() {
    log "Creating necessary directories..."
    mkdir -p logs backups config/ssl config/grafana/provisioning
    success "Directories created"
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."
    
    if ! command -v docker &> /dev/null; then
        error "Docker is not installed"
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        error "Docker Compose is not installed"
    fi
    
    if [ ! -f "$COMPOSE_FILE" ]; then
        error "Docker Compose file not found: $COMPOSE_FILE"
    fi
    
    success "Prerequisites check passed"
}

# Load environment variables
load_environment() {
    log "Loading environment variables..."
    
    if [ -f "$ENV_FILE" ]; then
        export $(cat "$ENV_FILE" | grep -v '^#' | xargs)
        success "Environment variables loaded from $ENV_FILE"
    else
        warning "Environment file not found: $ENV_FILE. Using defaults."
    fi
}

# Backup existing data
backup_data() {
    log "Creating backup..."
    
    BACKUP_TIMESTAMP=$(date +%Y%m%d_%H%M%S)
    BACKUP_PATH="$BACKUP_DIR/backup_$BACKUP_TIMESTAMP"
    
    mkdir -p "$BACKUP_PATH"
    
    # Backup SQL Server data if container exists
    if docker ps -a --format 'table {{.Names}}' | grep -q configmanager-sqlserver-prod; then
        log "Backing up SQL Server data..."
        docker exec configmanager-sqlserver-prod /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U sa -P "${SQL_SERVER_PASSWORD:-ConfigManager123!}" \
            -Q "BACKUP DATABASE [ConfigurationManager] TO DISK = '/tmp/configmanager_backup.bak'" || warning "SQL Server backup failed"
        
        docker cp configmanager-sqlserver-prod:/tmp/configmanager_backup.bak "$BACKUP_PATH/" || warning "Failed to copy SQL Server backup"
    fi
    
    # Backup MongoDB data if container exists
    if docker ps -a --format 'table {{.Names}}' | grep -q configmanager-mongo-prod; then
        log "Backing up MongoDB data..."
        docker exec configmanager-mongo-prod mongodump --out /tmp/mongo_backup || warning "MongoDB backup failed"
        docker cp configmanager-mongo-prod:/tmp/mongo_backup "$BACKUP_PATH/" || warning "Failed to copy MongoDB backup"
    fi
    
    success "Backup created at $BACKUP_PATH"
}

# Build and deploy
deploy() {
    log "Starting deployment..."
    
    # Pull latest images
    log "Pulling latest images..."
    docker-compose -f "$COMPOSE_FILE" pull
    
    # Build application images
    log "Building application images..."
    docker-compose -f "$COMPOSE_FILE" build --no-cache
    
    # Stop existing services
    log "Stopping existing services..."
    docker-compose -f "$COMPOSE_FILE" down
    
    # Start services
    log "Starting services..."
    docker-compose -f "$COMPOSE_FILE" up -d
    
    success "Deployment completed"
}

# Health check
health_check() {
    log "Performing health checks..."
    
    # Wait for services to start
    sleep 30
    
    # Check API health
    if curl -f http://localhost:8001/health > /dev/null 2>&1; then
        success "API health check passed"
    else
        error "API health check failed"
    fi
    
    # Check Blazor app
    if curl -f http://localhost:8002 > /dev/null 2>&1; then
        success "Blazor app health check passed"
    else
        error "Blazor app health check failed"
    fi
    
    # Check Nginx
    if curl -f http://localhost/health > /dev/null 2>&1; then
        success "Nginx health check passed"
    else
        error "Nginx health check failed"
    fi
    
    success "All health checks passed"
}

# Show status
show_status() {
    log "Deployment status:"
    docker-compose -f "$COMPOSE_FILE" ps
    
    echo ""
    log "Service URLs:"
    echo "  - Application: http://localhost"
    echo "  - API: http://localhost:8001"
    echo "  - Blazor App: http://localhost:8002"
    echo "  - Grafana: http://localhost:3000"
    echo "  - Prometheus: http://localhost:9090"
    echo "  - RabbitMQ Management: http://localhost:15672"
}

# Main execution
main() {
    log "Starting Configuration Manager deployment..."
    
    create_directories
    check_prerequisites
    load_environment
    
    # Ask for confirmation in production
    if [ "${ENVIRONMENT:-}" = "production" ]; then
        echo -e "${YELLOW}WARNING: You are deploying to PRODUCTION environment!${NC}"
        read -p "Are you sure you want to continue? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            log "Deployment cancelled by user"
            exit 0
        fi
        
        backup_data
    fi
    
    deploy
    health_check
    show_status
    
    success "Configuration Manager deployed successfully!"
}

# Handle script arguments
case "${1:-deploy}" in
    "deploy")
        main
        ;;
    "backup")
        create_directories
        load_environment
        backup_data
        ;;
    "status")
        show_status
        ;;
    "logs")
        docker-compose -f "$COMPOSE_FILE" logs -f "${2:-}"
        ;;
    "stop")
        log "Stopping services..."
        docker-compose -f "$COMPOSE_FILE" down
        success "Services stopped"
        ;;
    "restart")
        log "Restarting services..."
        docker-compose -f "$COMPOSE_FILE" restart "${2:-}"
        success "Services restarted"
        ;;
    *)
        echo "Usage: $0 {deploy|backup|status|logs|stop|restart}"
        echo "  deploy  - Deploy the application (default)"
        echo "  backup  - Create a backup of data"
        echo "  status  - Show service status"
        echo "  logs    - Show service logs (optionally specify service name)"
        echo "  stop    - Stop all services"
        echo "  restart - Restart services (optionally specify service name)"
        exit 1
        ;;
esac
