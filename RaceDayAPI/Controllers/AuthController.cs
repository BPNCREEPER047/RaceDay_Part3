using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db) => _db = db;

        /// <summary>Register a new user (Organiser or Participant).</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDto), 201)]
        [ProducesResponseType(400)][ProducesResponseType(409)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

            var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
            if (exists)
                return Conflict(new { message = "An account with this email already exists." });

            var user = new User
            {
                FullName  = dto.FullName.Trim(),
                Email     = dto.Email.ToLower().Trim(),
                Password  = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role      = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Auto-login: set session so the MVC can immediately use it
            HttpContext.Session.SetString(SessionKeys.UserId,    user.UserID.ToString());
            HttpContext.Session.SetString(SessionKeys.UserRole,  user.Role);
            HttpContext.Session.SetString(SessionKeys.UserEmail, user.Email);

            return CreatedAtAction(nameof(Register), new RegisterResponseDto
            {
                Message = "Registration successful.",
                User    = MapToDto(user)
            });
        }

        /// <summary>Log in with email and password. Creates a server-side session.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(400)][ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            HttpContext.Session.SetString(SessionKeys.UserId,    user.UserID.ToString());
            HttpContext.Session.SetString(SessionKeys.UserRole,  user.Role);
            HttpContext.Session.SetString(SessionKeys.UserEmail, user.Email);

            return Ok(new LoginResponseDto
            {
                Message = "Login successful.",
                User    = MapToDto(user)
            });
        }

        /// <summary>Log out by clearing the session.</summary>
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private static UserResponseDto MapToDto(User u) => new()
        {
            UserID    = u.UserID,
            FullName  = u.FullName,
            Email     = u.Email,
            Role      = u.Role,
            CreatedAt = u.CreatedAt
        };
    }
}
