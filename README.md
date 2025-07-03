# Configuration Manager

A comprehensive configuration management system built with .NET 8, Blazor Server, and Clean Architecture principles.

## Features

- **Configuration Management**: Create, update, and manage application configurations across multiple environments
- **Environment Management**: Organize configurations by environment (Development, Staging, Production, etc.)
- **Configuration Groups**: Hierarchical organization of configurations using groups and subgroups
- **Audit Logging**: Complete audit trail of all configuration changes
- **Real-time Updates**: Live configuration updates with caching and messaging
- **Security**: Encrypted configuration values and role-based access control
- **Monitoring**: Built-in health checks, metrics, and monitoring with Prometheus and Grafana

## Architecture

The application follows Clean Architecture principles with the following layers:

- **Domain**: Core business entities and rules
- **Application**: Use cases and business logic
- **Infrastructure**: Data access, external services, and cross-cutting concerns
- **API**: RESTful API endpoints
- **BlazorApp**: Web UI built with Blazor Server and MudBlazor
- **Client**: HTTP client services for API communication

## Technology Stack

- **.NET 8**: Core framework
- **Blazor Server**: Web UI framework
- **MudBlazor**: UI component library
- **Entity Framework Core**: ORM for SQL Server
- **MongoDB**: Document database for caching and audit logs
- **RabbitMQ**: Message broker for real-time updates
- **Serilog**: Structured logging
- **Docker**: Containerization
- **Nginx**: Reverse proxy and load balancer
- **Prometheus**: Metrics collection
- **Grafana**: Monitoring dashboards

## Quick Start

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK (for development)
- SQL Server (or use Docker Compose)
- MongoDB (or use Docker Compose)
- RabbitMQ (or use Docker Compose)

### Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ConfigurationManager
   ```

2. **Start infrastructure services**
   ```bash
   docker-compose up -d
   ```

3. **Run the API**
   ```bash
   cd src/Api
   dotnet run
   ```

4. **Run the Blazor App**
   ```bash
   cd src/BlazorApp
   dotnet run
   ```

5. **Access the application**
   - Blazor App: http://localhost:5000
   - API: http://localhost:5021
   - Swagger: http://localhost:5021/swagger

### Production Deployment

1. **Prepare environment file**
   ```bash
   cp .env.prod.template .env.prod
   # Edit .env.prod with your production values
   ```

2. **Deploy using PowerShell (Windows)**
   ```powershell
   .\scripts\deploy.ps1 deploy
   ```

3. **Deploy using Bash (Linux/macOS)**
   ```bash
   chmod +x scripts/deploy.sh
   ./scripts/deploy.sh deploy
   ```

4. **Access the application**
   - Application: http://localhost
   - API: http://localhost:8001
   - Blazor App: http://localhost:8002
   - Grafana: http://localhost:3000
   - Prometheus: http://localhost:9090
   - RabbitMQ Management: http://localhost:15672

## Configuration

### Environment Variables

Key environment variables for production:

- `SQL_SERVER_PASSWORD`: SQL Server SA password
- `MONGO_PASSWORD`: MongoDB admin password
- `RABBITMQ_PASSWORD`: RabbitMQ admin password
- `JWT_SECRET`: JWT signing secret (minimum 32 characters)
- `GRAFANA_PASSWORD`: Grafana admin password

See `.env.prod.template` for all available configuration options.

### Database Connection Strings

- **SQL Server**: `Server=localhost,1433;Database=ConfigurationManager;User Id=sa;Password=<password>;TrustServerCertificate=true;`
- **MongoDB**: `mongodb://admin:<password>@localhost:27017`
- **RabbitMQ**: `amqp://admin:<password>@localhost:5672`

## API Documentation

The API is documented using OpenAPI/Swagger. Access the documentation at:
- Development: http://localhost:5021/swagger
- Production: http://localhost:8001/swagger

### Key Endpoints

- `GET /api/v1/configurations` - List configurations
- `POST /api/v1/configurations` - Create configuration
- `GET /api/v1/environments` - List environments
- `GET /api/v1/groups` - List configuration groups
- `GET /api/v1/audit` - View audit logs
- `GET /health` - Health check endpoint

## Monitoring

### Health Checks

Health checks are available at `/health` and monitor:
- SQL Server connectivity
- MongoDB connectivity
- RabbitMQ connectivity
- Application health

### Metrics

Prometheus metrics are exposed at `/metrics` and include:
- HTTP request metrics
- Database connection metrics
- Custom business metrics

### Dashboards

Grafana dashboards provide monitoring for:
- Application performance
- Infrastructure health
- Business metrics
- Error rates and response times

## Security

### Authentication

The application supports JWT-based authentication with:
- User registration and login
- Role-based access control
- Token refresh mechanism
- Password hashing with bcrypt

### Authorization

Role-based permissions include:
- **Administrator**: Full system access
- **Configuration Manager**: Manage configurations and environments
- **Viewer**: Read-only access
- **Auditor**: Access to audit logs

### Data Protection

- Sensitive configuration values are encrypted at rest
- All API communications use HTTPS in production
- Audit logging tracks all changes
- Rate limiting prevents abuse

## Development

### Project Structure

```
src/
├── Api/                 # Web API project
├── Application/         # Application layer (use cases)
├── BlazorApp/          # Blazor Server UI
├── Client/             # HTTP client services
├── Domain/             # Domain entities and rules
└── Infrastructure/     # Data access and external services

config/                 # Configuration files
scripts/               # Deployment scripts
docker-compose.yml     # Development infrastructure
docker-compose.prod.yml # Production deployment
```

### Running Tests

```bash
dotnet test
```

### Code Quality

The project includes:
- EditorConfig for consistent formatting
- Analyzers for code quality
- FluentValidation for input validation
- Structured logging with Serilog

## Deployment Scripts

### PowerShell (Windows)

```powershell
# Deploy application
.\scripts\deploy.ps1 deploy

# Create backup
.\scripts\deploy.ps1 backup

# View status
.\scripts\deploy.ps1 status

# View logs
.\scripts\deploy.ps1 logs [service-name]

# Stop services
.\scripts\deploy.ps1 stop

# Restart services
.\scripts\deploy.ps1 restart [service-name]
```

### Bash (Linux/macOS)

```bash
# Deploy application
./scripts/deploy.sh deploy

# Create backup
./scripts/deploy.sh backup

# View status
./scripts/deploy.sh status

# View logs
./scripts/deploy.sh logs [service-name]

# Stop services
./scripts/deploy.sh stop

# Restart services
./scripts/deploy.sh restart [service-name]
```

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Check connection strings in configuration
   - Ensure SQL Server is running and accessible
   - Verify credentials

2. **MongoDB Connection Failed**
   - Check MongoDB connection string
   - Ensure MongoDB is running
   - Verify authentication credentials

3. **RabbitMQ Connection Failed**
   - Check RabbitMQ connection string
   - Ensure RabbitMQ is running
   - Verify management plugin is enabled

4. **Health Check Failures**
   - Check service logs: `docker-compose logs [service-name]`
   - Verify all dependencies are running
   - Check network connectivity between containers

### Logs

Application logs are available in:
- Container logs: `docker-compose logs -f [service-name]`
- File logs: `./logs/` directory
- Structured logs in Grafana/Loki (if configured)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation
- Review the troubleshooting section
# configuration-manager
