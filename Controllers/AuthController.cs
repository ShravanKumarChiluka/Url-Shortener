using Microsoft.AspNetCore.Mvc;
using UrlShortener.DTOs;
using UrlShortener.Services;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController:ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (result == null)
            {
                return Conflict(new { message = "Email already exists" });
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if(result == null)
            {
                return Conflict(new { message = "Invalid Credentials" });
            }
            return Ok(result);
        }
    }
}
