using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api")]
public class HelloController : ControllerBase
{
    private readonly IDatabaseHealthService _databaseHealthService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HelloController> _logger;

    public HelloController(
        IDatabaseHealthService databaseHealthService,
        IUnitOfWork unitOfWork,
        ILogger<HelloController> logger)
    {
        _databaseHealthService = databaseHealthService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("hello")]
    public async Task<ActionResult<string>> Hello()
    {
        _logger.LogInformation("Hello endpoint called");
        return Ok("Hello, World!");
    }

    [HttpGet("hello.json")]
    public async Task<ActionResult<HelloResponse>> HelloJson()
    {
        _logger.LogInformation("Hello.json endpoint called");
        
        var isHealthy = await _databaseHealthService.IsHealthyAsync();
        var response = new HelloResponse("Hello, World!", isHealthy);
        
        return Ok(response);
    }

    [HttpGet("health")]
    public async Task<ActionResult<HealthResponse>> Health()
    {
        _logger.LogInformation("Health endpoint called");
        
        var isHealthy = await _databaseHealthService.IsHealthyAsync();
        string? databaseInfo = null;
        
        if (isHealthy)
        {
            try
            {
                databaseInfo = await _databaseHealthService.GetDatabaseInfoAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get database info");
            }
        }
        
        var response = new HealthResponse(isHealthy, databaseInfo);
        return Ok(response);
    }

    [HttpGet("health.json")]
    public async Task<ActionResult<HealthResponse>> HealthJson()
    {
        return await Health();
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        _logger.LogInformation("GetAllUsers endpoint called");
        
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var userDtos = users.Select(u => new UserDto(u)).ToList();
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users.json")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsersJson()
    {
        return await GetAllUsers();
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(long id)
    {
        _logger.LogInformation("GetUserById endpoint called with id: {Id}", id);
        
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var userDto = new UserDto(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users/{id}.json")]
    public async Task<ActionResult<UserDto>> GetUserByIdJson(long id)
    {
        return await GetUserById(id);
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("CreateUser endpoint called");
        
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Username and email are required");
            }

            // Check if username or email already exists
            if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
            {
                return Conflict("Username already exists");
            }

            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                return Conflict("Email already exists");
            }

            var user = new Core.Entities.User(request.Username, request.Email);
            var createdUser = await _unitOfWork.Users.AddAsync(user);
            
            var userDto = new UserDto(createdUser);
            return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("users.json")]
    public async Task<ActionResult<UserDto>> CreateUserJson([FromBody] CreateUserRequest request)
    {
        return await CreateUser(request);
    }

    [HttpGet("users/count")]
    public async Task<ActionResult<CountResponse>> GetUserCount()
    {
        _logger.LogInformation("GetUserCount endpoint called");
        
        try
        {
            var count = await _unitOfWork.Users.CountAsync();
            var response = new CountResponse(count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users/count.json")]
    public async Task<ActionResult<CountResponse>> GetUserCountJson()
    {
        return await GetUserCount();
    }
}