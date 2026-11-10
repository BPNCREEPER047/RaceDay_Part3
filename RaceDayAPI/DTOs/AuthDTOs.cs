using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    // ── Auth ──────────────────────────────────────────────────────────────────
    public class RegisterDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // LoginResponseDto - MVC reads "user" from this after login
    public class LoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = new();
    }

    // RegisterResponseDto - MVC reads "user" from this after register
    public class RegisterResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = new();
    }

    // ── Profile ───────────────────────────────────────────────────────────────
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        [EmailAddress] public string? Email { get; set; }
        [MinLength(6)] public string? Password { get; set; }
    }
}
