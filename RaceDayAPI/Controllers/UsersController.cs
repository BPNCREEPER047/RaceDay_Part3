using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Returns the profile of the currently logged-in user.</summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetProfile()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            var userId = HttpContext.Session.GetUserId()!.Value;
            var user   = await _db.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new UserResponseDto
            {
                UserID    = user.UserID,
                FullName  = user.FullName,
                Email     = user.Email,
                Role      = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>Updates the currently logged-in user's profile details.</summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetUserId()!.Value;
            var user   = await _db.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            // Only update fields that were provided
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailTaken = await _db.Users
                    .AnyAsync(u => u.Email == dto.Email.ToLower() && u.UserID != userId);
                if (emailTaken)
                    return BadRequest(new { message = "Email is already in use." });

                user.Email = dto.Email.ToLower().Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _db.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                UserID    = user.UserID,
                FullName  = user.FullName,
                Email     = user.Email,
                Role      = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>Returns a list of all users. Organiser access only.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can view the full user list." });

            var users = await _db.Users
                .OrderBy(u => u.FullName)
                .Select(u => new UserResponseDto
                {
                    UserID    = u.UserID,
                    FullName  = u.FullName,
                    Email     = u.Email,
                    Role      = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
