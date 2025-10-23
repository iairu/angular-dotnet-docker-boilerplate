using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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
            .AddDbContextCheck<AppDbContext>("database");

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

            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
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

            c.EnableAnnotations();
        });

        return services;
    }
}