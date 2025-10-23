using API.Extensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Core.Interfaces;
using Infrastructure.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Database initialization and health check on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Starting database initialization...");
        
        // Ensure database is created and apply migrations
        var context = services.GetRequiredService<AppDbContext>();
        
        // Wait for database to be ready (retry logic)
        var retryCount = 0;
        var maxRetries = 30; // 30 seconds total wait time
        
        while (retryCount < maxRetries)
        {
            try
            {
                await context.Database.CanConnectAsync();
                break;
            }
            catch (Exception)
            {
                retryCount++;
                logger.LogInformation($"⏳ Waiting for database... (attempt {retryCount}/{maxRetries})");
                await Task.Delay(1000); // Wait 1 second
                
                if (retryCount >= maxRetries)
                {
                    throw new TimeoutException("Database connection timeout after 30 seconds");
                }
            }
        }
        
        // Verify migrations and table existence
        await VerifyAndApplyMigrations(context, logger);
        
        // Test database connection
        var healthService = services.GetRequiredService<IDatabaseHealthService>();
        await healthService.TestConnectionAsync();
        
        // Test Entity Framework functionality
        await TestEntityFunctionality(services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred during database initialization");
        logger.LogWarning("🚀 Application will continue running without database initialization");
    }
}

var appLogger = app.Services.GetRequiredService<ILogger<Program>>();
appLogger.LogInformation("🚀 Angular .NET Docker Boilerplate API is starting...");
appLogger.LogInformation("📡 API will be available at: http://localhost:8080");
appLogger.LogInformation("📖 Swagger documentation: http://localhost:8080/swagger");
appLogger.LogInformation("🔍 Health check: http://localhost:8080/health");

app.Run();

async Task TestEntityFunctionality(IServiceProvider services, ILogger<Program> logger)
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
            try
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
            catch (Exception createEx)
            {
                logger.LogWarning(createEx, "⚠️ Could not create test user");
            }
        }
        else
        {
            // Show existing users (limit to prevent log spam)
            var allUsers = await unitOfWork.Users.GetAllAsync();
            var userList = allUsers.Take(5).ToList();
            logger.LogInformation("📋 Existing users (showing first 5):");
            foreach (var user in userList)
            {
                logger.LogInformation("   - {Username} ({Email})", user.Username, user.Email);
            }
            if (allUsers.Count > 5)
            {
                logger.LogInformation("   ... and {Count} more users", allUsers.Count - 5);
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

async Task VerifyAndApplyMigrations(AppDbContext context, ILogger<Program> logger)
{
    try
    {
        logger.LogInformation("🔍 Verifying database schema and migrations...");
        
        // List of all expected tables from migrations
        var expectedTables = new[] { "users" };
        
        // Check if all expected tables exist
        var missingTables = new List<string>();
        
        foreach (var tableName in expectedTables)
        {
            var tableExists = await context.Database.ExecuteSqlRawAsync(
                $"SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '{tableName}'") >= 0;
            
            // Alternative check using raw query
            var sql = $@"
                SELECT EXISTS (
                    SELECT 1 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = '{tableName}'
                )";
            
            var exists = false;
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                await context.Database.OpenConnectionAsync();
                
                var result = await command.ExecuteScalarAsync();
                exists = result != null && (bool)result;
            }
            
            if (!exists)
            {
                logger.LogWarning($"⚠️ Table '{tableName}' does not exist in database");
                missingTables.Add(tableName);
            }
            else
            {
                logger.LogInformation($"✅ Table '{tableName}' exists");
            }
        }
        
        // If any tables are missing, reset and reapply migrations
        if (missingTables.Any())
        {
            logger.LogWarning("❌ Database schema is incomplete. Missing tables: {Tables}", string.Join(", ", missingTables));
            logger.LogInformation("🔄 Resetting migration history and reapplying all migrations...");
            
            // Drop the migration history table to force reapplication
            try
            {
                await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE");
                logger.LogInformation("🗑️ Dropped migration history table");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not drop migration history table (may not exist)");
            }
            
            // Reapply all migrations
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ All migrations reapplied successfully");
            
            // Verify again
            logger.LogInformation("🔍 Verifying tables after migration...");
            foreach (var tableName in expectedTables)
            {
                var sql = $@"
                    SELECT EXISTS (
                        SELECT 1 
                        FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_name = '{tableName}'
                    )";
                
                using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    if (context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                    {
                        await context.Database.OpenConnectionAsync();
                    }
                    
                    var result = await command.ExecuteScalarAsync();
                    var exists = result != null && (bool)result;
                    
                    if (exists)
                    {
                        logger.LogInformation($"✅ Verified: Table '{tableName}' now exists");
                    }
                    else
                    {
                        logger.LogError($"❌ CRITICAL: Table '{tableName}' still does not exist after migration!");
                    }
                }
            }
        }
        else
        {
            // All tables exist, just run normal migration check
            logger.LogInformation("✅ All expected tables exist in database");
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Database migrations verified and up to date");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during migration verification");
        // Try to apply migrations anyway as fallback
        logger.LogInformation("🔄 Attempting to apply migrations despite verification error...");
        await context.Database.MigrateAsync();
        logger.LogInformation("✅ Migrations applied (verification skipped due to error)");
    }
}