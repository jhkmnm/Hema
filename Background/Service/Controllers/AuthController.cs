using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (await _userService.ValidateUserAsync(request.Username, request.Password))
        {
            return Ok(new { message = "登录成功" });
        }
        return Unauthorized(new { message = "用户名或密码错误" });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
} 
