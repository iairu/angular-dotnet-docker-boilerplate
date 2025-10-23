namespace Core.DTOs;

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public ApiResponse()
    {
    }

    public ApiResponse(T data)
    {
        Data = data;
        Success = true;
    }

    public ApiResponse(T data, string message) : this(data)
    {
        Message = message;
    }

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>(data);
    }

    public static ApiResponse<T> SuccessResponse(T data, string message)
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse<T> ErrorResponse(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message
        };
    }
}

public class HelloResponse
{
    public string Message { get; set; } = string.Empty;
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public bool DatabaseHealthy { get; set; } = false;

    public HelloResponse()
    {
    }

    public HelloResponse(string message, bool databaseHealthy = false)
    {
        Message = message;
        DatabaseHealthy = databaseHealthy;
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string? DatabaseInfo { get; set; }

    public HealthResponse()
    {
    }

    public HealthResponse(bool isHealthy, string? databaseInfo = null)
    {
        Status = isHealthy ? "UP" : "DOWN";
        Database = isHealthy ? "CONNECTED" : "DISCONNECTED";
        DatabaseInfo = databaseInfo;
    }
}

public class CountResponse
{
    public long Count { get; set; }
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public CountResponse()
    {
    }

    public CountResponse(long count)
    {
        Count = count;
    }
}

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserDto()
    {
    }

    public UserDto(Entities.User user)
    {
        Id = user.Id;
        Username = user.Username;
        Email = user.Email;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}