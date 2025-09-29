# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

This is a parking management system (Sistema de Parqueaderos) built with ASP.NET Core 8.0 MVC and MySQL. The application manages parking spaces, vehicle registrations, reservations, and user authentication with role-based access control.

## Common Development Commands

### Building and Running
```bash
# Build the project
dotnet build WebApplication1/WebApplication1.csproj

# Run the application (development mode)
dotnet run --project WebApplication1/WebApplication1.csproj

# Run with specific profile
dotnet run --project WebApplication1/WebApplication1.csproj --launch-profile https
```

### Database Operations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project WebApplication1

# Update database
dotnet ef database update --project WebApplication1

# Drop database (careful!)
dotnet ef database drop --project WebApplication1

# Generate DbContext scaffold (if needed)
dotnet ef dbcontext scaffold "server=localhost;port=3306;database=ParqueaderoDB;user=root;password=carlosmanuel" Pomelo.EntityFrameworkCore.MySql --project WebApplication1 --output-dir Models --context-dir Data
```

### Package Management
```bash
# Restore packages
dotnet restore WebApplication1/WebApplication1.csproj

# Add package
dotnet add WebApplication1/WebApplication1.csproj package PackageName

# List packages
dotnet list WebApplication1/WebApplication1.csproj package
```

### Testing and Code Generation
```bash
# Generate controller with views (requires existing model)
dotnet aspnet-codegenerator controller -name ModelNameController -m ModelName -dc ApplicationDbContext --relativeFolderPath Controllers --useDefaultLayout --referenceScriptLibraries -f

# Run single test (when tests are added)
dotnet test WebApplication1.Tests --filter "TestMethodName"
```

## Architecture Overview

### Domain Models
The application manages a parking system with the following core entities:

- **ApplicationUser** (extends IdentityUser): Users with custom `Nombre` property
- **Vehiculos**: User-owned vehicles with plate and type
- **Parqueos**: Active parking sessions with entry/exit times and billing
- **Reservas**: Time-based parking reservations
- **Tarifas**: Pricing configuration by vehicle type and location

### Key Relationships
- Users can own multiple vehicles (1:N)
- Vehicles can have multiple parking sessions and reservations (1:N)
- Parking sessions link users, vehicles, and calculate billing
- Reservations have expiration times and active states

### Authentication & Authorization
- Uses ASP.NET Core Identity with MySQL backend
- Two predefined roles: "Aprendiz" and "Funcionario"
- Roles are automatically seeded on application startup
- Authentication required for parking operations

### Database Configuration
- **Provider**: MySQL 8.0.36 via Pomelo.EntityFrameworkCore.MySql
- **Connection**: Configured in appsettings.json (currently localhost:3306)
- **Migrations**: Located in WebApplication1/Migrations/
- **Context**: ApplicationDbContext extends IdentityDbContext

### MVC Structure
- **Controllers**: Standard CRUD operations for all entities
- **Views**: Razor pages with Bootstrap styling, includes Identity UI
- **Models**: Entity models with required navigation properties
- **Data**: DbContext and seeding logic in MyApp.Data namespace

### Key Technical Patterns
1. **Repository Pattern**: Through Entity Framework DbContext
2. **MVC Pattern**: Controllers return views with strongly-typed models
3. **Code-First Migrations**: Database schema managed through EF migrations
4. **Dependency Injection**: Services configured in Program.cs
5. **Role-Based Security**: Identity framework integration

### Configuration Notes
- MySQL connection string contains hardcoded credentials (development only)
- Entity Framework tools required for migrations and scaffolding
- ASP.NET Core Identity UI provides authentication pages
- Application runs on HTTPS (port 7056) and HTTP (port 5291) in development

### Development Workflow
1. Model changes require new migrations (`dotnet ef migrations add`)
2. Database updates applied with `dotnet ef database update`
3. Controllers are scaffolded with full CRUD operations
4. Views follow consistent Bootstrap-based layouts
5. Role seeding happens automatically on application start

### Important Namespaces
- `WebApplication1.Models`: Domain entities
- `MyApp.Data`: DbContext and data seeding
- `WebApplication1.Controllers`: MVC controllers

### Database Schema Evolution
The system evolved from a basic user/role system to ASP.NET Core Identity, evidenced by the "AddIdentityTables" migration which created the full Identity table structure while maintaining custom domain tables.