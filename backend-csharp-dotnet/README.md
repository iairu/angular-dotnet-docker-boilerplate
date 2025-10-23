# .NET Backend

This is the ASP.NET Core backend application for the Angular + .NET + Docker boilerplate, built with Clean Architecture principles.

## 🚀 Features

- **ASP.NET Core 8.0**: Modern web API framework with minimal APIs
- **Clean Architecture**: Separation of concerns with Core, Infrastructure, and API layers
- **Entity Framework Core**: ORM with PostgreSQL support and automatic migrations
- **Repository Pattern**: Data access abstraction with Unit of Work
- **Health Checks**: Database connectivity monitoring and diagnostics
- **Swagger/OpenAPI**: Comprehensive API documentation
- **CORS Configuration**: Cross-origin request support for Angular frontend
- **Structured Logging**: Serilog with console and file outputs
- **Docker Ready**: Multi-stage Dockerfile for production deployment

## 📋 Prerequisites

- .NET 8.0 SDK or higher
- PostgreSQL (or Docker with PostgreSQL container)
- Visual Studio 2022 / VS Code / JetBrains Rider (optional)

## 🏗 Project Structure

```
src/
├── API/                          # Web API layer
│   ├── Controllers/              # REST API controllers
│   ├── Extensions/               # Service configuration extensions
│   ├── Program.cs               # Application entry point
│   └── appsettings.json         # Configuration files
├── Core/                        # Domain layer
│   ├── Entities/                # Domain entities
│   ├── Interfaces/              # Repository and service interfaces
│   └── DTOs/                    # Data transfer objects
└── Infrastructure/              # Data access layer
    ├── Data/                    # DbContext and configurations
    ├── Repositories/            # Repository implementations
    ├── Services/                # Infrastructure services
    └── Migrations/              # Entity Framework migrations
```

## 🛠 Development

### Local Development (without Docker)

1. **Install Dependencies**
   ```bash
   dotnet restore
   ```

2. **Update Database Connection**
   
   Update `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=boilerplate_db;Username=your_user;Password=your_password"
     }
   }
   ```

3. **Run Database Migrations**
   ```bash
   cd src/API
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   cd src/API
   dotnet run
   ```

   The API will be available at:
   - **Swagger UI**: http://localhost:5000/swagger
   - **API Base**: http://localhost:5000/api
   - **Health Check**: http://localhost:5000/health

### Docker Development

The backend runs automatically as part of the Docker Compose setup from the root directory:

```bash
# From project root
docker compose up
```

## 🔌 API Endpoints

### Health & Status
- `GET /api/hello` - Simple hello message
- `GET /api/hello.json` - Hello message with database status
- `GET /api/health` - Database health check
- `GET /api/health.json` - Database health check (JSON format)

### User Management
- `GET /api/users` - Get all users
- `GET /api/users.json` - Get all users (JSON format)
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/{id}.json` - Get user by ID (JSON format)
- `POST /api/users` - Create new user
- `POST /api/users.json` - Create new user (JSON format)
- `GET /api/users/count` - Get user count
- `GET /api/users/count.json` - Get user count (JSON format)

## 🗄️ Database

### Entity Framework Core
- **Provider**: Npgsql (PostgreSQL)
- **Migrations**: Automatic on application startup
- **Connection**: Configured via connection strings

### User Entity
```csharp
public class User
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Migration Commands
```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/API

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/API

# Remove last migration
dotnet ef migrations remove --project src/Infrastructure --startup-project src/API
```

## 🔧 Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ASPNETCORE_URLS` - Server URLs (default: http://+:8080)
- `ConnectionStrings__DefaultConnection` - Database connection string

### Docker Configuration
The application is configured to work seamlessly with Docker:
- **Port**: 8080 (internal), mapped via docker-compose
- **Database**: PostgreSQL container (postgres:18.0-alpine)
- **Logging**: Console and file outputs
- **Health Checks**: Built-in EF Core health checks

## 📦 Technologies

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 18.0
- **Documentation**: Swagger/OpenAPI
- **Logging**: Serilog
- **Testing**: xUnit (planned)
- **Containerization**: Docker

## 🐛 Troubleshooting

### Common Issues

**Database connection fails:**
```bash
# Check PostgreSQL container
docker compose logs postgres

# Check connection string in appsettings
# Verify database is running and accessible
```

**Migration errors:**
```bash
# Reset database (development only)
docker compose down -v
docker compose up postgres -d
dotnet ef database update
```

**Port conflicts:**
```bash
# Check what's using port 8080
sudo lsof -i :8080

# Change port in docker-compose.yml if needed
```

## 🚀 Deployment

### Production Build
```bash
# Build for production
dotnet publish src/API/API.csproj -c Release -o ./publish

# Run published version
cd publish
dotnet API.dll
```

### Docker Production
```bash
# Build production image
docker build -t angular-dotnet-backend .

# Run production container
docker run -p 8080:8080 angular-dotnet-backend
```

---

Part of the [Angular + .NET + Docker Boilerplate](https://github.com/iairu/angular-dotnet-docker-boilerplate)