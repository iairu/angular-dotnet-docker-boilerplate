using API.Extensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Core.Interfaces;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Angular .NET Docker Boilerplate API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

app.UseHttpsRedirection();

// CORS - must be before routing
app.UseCors("AllowSpecificOrigin");

app.UseRouting();
app.UseAuthorization();

// Health check endpoint
app.UseHealthChecks("/health");

app.MapControllers();

// Database initialization and health check on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Ensure database is created and apply migrations
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        
        logger.LogInformation("✅ Database migrations applied successfully");
        
        // Test database connection
        var healthService = services.GetRequiredService<IDatabaseHealthService>();
        await healthService.TestConnectionAsync();
        
        // Test JPA functionality equivalent
        await TestEntityFunctionality(services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred during database initialization");
    }
}

logger.LogInformation("🚀 Angular .NET Docker Boilerplate API is starting...");
logger.LogInformation("📡 API will be available at: http://localhost:8080");
logger.LogInformation("📖 Swagger documentation: http://localhost:8080/swagger");
logger.LogInformation("🔍 Health check: http://localhost:8080/health");

app.Run();

async Task TestEntityFunctionality(IServiceProvider services, ILogger logger)
{
    try
    {
        logger.LogInformation("==========================================");
        logger.LogInformation("🔧 ENTITY FRAMEWORK FUNCTIONALITY TEST");
        logger.LogInformation("==========================================");

        var unitOfWork = services.GetRequiredService<IUnitOfWork>();

        // Count existing users
        var userCount = await unitOfWork.Users.CountAsync();
        logger.LogInformation("👥 Current user count: {UserCount}", userCount);

        // Create a test user if none exist
        if (userCount == 0)
        {
            var testUser = new Core.Entities.User("testuser", "test@example.com");
            var savedUser = await unitOfWork.Users.AddAsync(testUser);
            logger.LogInformation("✅ Created test user: {User}", savedUser);

            // Verify the user was saved
            var foundUser = await unitOfWork.Users.GetByUsernameAsync("testuser");
            if (foundUser != null)
            {
                logger.LogInformation("✅ User retrieval successful: {Username}", foundUser.Username);
            }
            else
            {
                logger.LogWarning("❌ Failed to retrieve saved user");
            }
        }
        else
        {
            // Show existing users
            var allUsers = await unitOfWork.Users.GetAllAsync();
            logger.LogInformation("📋 Existing users:");
            foreach (var user in allUsers)
            {
                logger.LogInformation("   - {Username} ({Email})", user.Username, user.Email);
            }
        }

        // Test custom query - users created in last 24 hours
        var recentUsers = await unitOfWork.Users.GetUsersCreatedAfterAsync(DateTime.UtcNow.AddDays(-1));
        logger.LogInformation("📅 Users created in last 24 hours: {RecentUserCount}", recentUsers.Count);

        logger.LogInformation("✅ Entity Framework functionality test completed successfully!");
        logger.LogInformation("==========================================");
    }
    catch (Exception ex)
    {
        logger.LogError("==========================================");
        logger.LogError("❌ ENTITY FRAMEWORK FUNCTIONALITY TEST FAILED");
        logger.LogError("==========================================");
        logger.LogError(ex, "Error: {Message}", ex.Message);
        logger.LogError("==========================================");
    }
}