using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Infrastructure.Services;

public class DatabaseHealthService : IDatabaseHealthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseHealthService> _logger;

    public DatabaseHealthService(AppDbContext context, ILogger<DatabaseHealthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5; // 5 second timeout
            
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Database health check failed: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<string> GetDatabaseInfoAsync()
    {
        try
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var serverVersion = connection.ServerVersion;
            var database = connection.Database;
            var dataSource = connection.DataSource;
            
            return $"Database: {database}, Server: {dataSource}, Version: {serverVersion}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database info: {Message}", ex.Message);
            return "Database info unavailable";
        }
    }

    public async Task TestConnectionAsync()
    {
        try
        {
            _logger.LogInformation("==========================================");
            _logger.LogInformation("🗄️  DATABASE CONNECTION TEST");
            _logger.LogInformation("==========================================");

            var connection = _context.Database.GetDbConnection();
            
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            // Get database metadata
            var serverVersion = connection.ServerVersion;
            var database = connection.Database;
            var dataSource = connection.DataSource;
            var connectionString = connection.ConnectionString;

            _logger.LogInformation("✅ Database connection successful!");
            _logger.LogInformation("📊 Database Product Name: PostgreSQL");
            _logger.LogInformation("📋 Database Product Version: {ServerVersion}", serverVersion);
            _logger.LogInformation("🔗 Connection DataSource: {DataSource}", dataSource);
            _logger.LogInformation("🏠 Database Name: {Database}", database);
            _logger.LogInformation("⚙️  Driver Name: Npgsql");

            // Test basic query
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT current_database(), version(), current_user";
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var currentDb = reader.GetString(0);
                var version = reader.GetString(1);
                var currentUser = reader.GetString(2);
                
                _logger.LogInformation("🔍 Current Database: {CurrentDb}", currentDb);
                _logger.LogInformation("👤 Current User: {CurrentUser}", currentUser);
                _logger.LogInformation("📄 Full Version: {Version}", version.Substring(0, Math.Min(version.Length, 100)));
            }

            _logger.LogInformation("==========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError("==========================================");
            _logger.LogError("❌ DATABASE CONNECTION FAILED");
            _logger.LogError("==========================================");
            _logger.LogError(ex, "Error: {Message}", ex.Message);
            _logger.LogError("==========================================");
            throw;
        }
    }
}