using JobApplication.DTOs.Auth;
using JobApplication.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) 
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result == null)
            {
                return Conflict(new { message = "A regisztráció sikertelen. Valószínűleg az email már használatban van." });
            }

            return CreatedAtAction(nameof(Register), new { email = dto.Email }, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new { message = "Hibás email vagy jelszó." });
            }

            return Ok(result);
        }
    }
}