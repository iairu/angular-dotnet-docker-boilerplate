using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });

        // Repository pattern
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
        
        // Health checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", policy =>
            {
                policy.WithOrigins(
                    "http://localhost",
                    "http://localhost:4200",
                    "http://localhost:3000",
                    "http://localhost:8080"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Angular .NET Docker Boilerplate API",
                Version = "v1",
                Description = "A REST API for the Angular + .NET + Docker boilerplate application",
                Contact = new()
                {
                    Name = "Ondrej Špánik",
                    Email = "contact@iairu.com",
                    Url = new Uri("https://iairu.com")
                }
            });
        });

        return services;
    }
}

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDatabaseHealthService _databaseHealthService;

    public DatabaseHealthCheck(IDatabaseHealthService databaseHealthService)
    {
        _databaseHealthService = databaseHealthService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _databaseHealthService.IsHealthyAsync();
            if (isHealthy)
            {
                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            return HealthCheckResult.Unhealthy("Database connection failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}", ex);
        }
    }
}