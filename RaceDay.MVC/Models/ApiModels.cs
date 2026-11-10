namespace RaceDay.MVC.Models
{
    // ── Auth ──────────────────────────────────────────────────────────────────
    public class RegisterViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role     { get; set; } = "Participant";
    }

    public class LoginViewModel
    {
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // What the MVC stores after a successful login/register
    public class AuthResponseDto
    {
        public UserDto User { get; set; } = null!;
    }

    // ── User ──────────────────────────────────────────────────────────────────
    public class UserDto
    {
        public int      UserID    { get; set; }
        public string   FullName  { get; set; } = string.Empty;
        public string   Email     { get; set; } = string.Empty;
        public string   Role      { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Sent to API PUT /api/users/profile
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Email    { get; set; }
        public string? Password { get; set; }
    }

    public class UpdateProfileViewModel
    {
        public string?    FullName       { get; set; }
        public string?    Email          { get; set; }
        public string?    Password       { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }

    // ── Category ──────────────────────────────────────────────────────────────
    public class CategoryDto
    {
        public int     CategoryID   { get; set; }
        public string  CategoryName { get; set; } = string.Empty;
        public string? Description  { get; set; }
    }

    public class CreateCategoryViewModel
    {
        public string  CategoryName { get; set; } = string.Empty;
        public string? Description  { get; set; }
    }

    // ── Event ─────────────────────────────────────────────────────────────────
    public class EventDto
    {
        public int      EventID        { get; set; }
        public string   EventName      { get; set; } = string.Empty;
        public string?  Description    { get; set; }
        public DateTime EventDate      { get; set; }
        public string   Location       { get; set; } = string.Empty;
        public decimal  Distance       { get; set; }
        public string   EventType      { get; set; } = "run";
        public int      MaxParticipants { get; set; }
        public int      CategoryID     { get; set; }
        public string?  CategoryName   { get; set; }
        public int      OrganiserID    { get; set; }
        public string?  OrganiserName  { get; set; }
        public DateTime CreatedAt      { get; set; }
        public string?  BannerImageUrl { get; set; }
    }

    // Sent to API POST/PUT /api/events
    public class CreateEventApiDto
    {
        public string   EventName       { get; set; } = string.Empty;
        public string?  Description     { get; set; }
        public DateTime EventDate       { get; set; }
        public string   Location        { get; set; } = string.Empty;
        public decimal  Distance        { get; set; }
        public string   EventType       { get; set; } = "run";
        public int      MaxParticipants { get; set; } = 100;
        public int      CategoryID      { get; set; }
        public string?  BannerImageUrl  { get; set; }
    }

    // MVC form view models
    public class CreateEventViewModel
    {
        public string        EventName       { get; set; } = string.Empty;
        public string?       Description     { get; set; }
        public DateTime      EventDate       { get; set; } = DateTime.Now.AddDays(30);
        public string        Location        { get; set; } = string.Empty;
        public decimal       Distance        { get; set; }
        public string        EventType       { get; set; } = "run";
        public int           MaxParticipants { get; set; } = 100;
        public int           CategoryID      { get; set; }
        public IFormFile?    BannerImage     { get; set; }
        public List<CategoryDto> Categories  { get; set; } = new();
    }

    public class EditEventViewModel
    {
        public int           EventID         { get; set; }
        public string        EventName       { get; set; } = string.Empty;
        public string?       Description     { get; set; }
        public DateTime      EventDate       { get; set; }
        public string        Location        { get; set; } = string.Empty;
        public decimal       Distance        { get; set; }
        public string        EventType       { get; set; } = "run";
        public int           MaxParticipants { get; set; }
        public int           CategoryID      { get; set; }
        public IFormFile?    BannerImage     { get; set; }
        public string?       ExistingBannerUrl { get; set; }
        public List<CategoryDto> Categories  { get; set; } = new();
    }

    // ── Enrolment ─────────────────────────────────────────────────────────────
    public class EnrolmentDto
    {
        public int      EnrolmentID     { get; set; }
        public int      ParticipantID   { get; set; }
        public string?  ParticipantName { get; set; }
        public int      EventID         { get; set; }
        public string?  EventName       { get; set; }
        public string?  CategoryName    { get; set; }
        public DateTime EnrolledAt      { get; set; }
        public string   Status          { get; set; } = string.Empty;
    }

    // ── Result ────────────────────────────────────────────────────────────────
    public class ResultDto
    {
        public int      ResultID        { get; set; }
        public int      EnrolmentID     { get; set; }
        public string?  ParticipantName { get; set; }
        public string?  EventName       { get; set; }
        public DateTime? EventDate      { get; set; }
        public string?  CategoryName    { get; set; }
        public string   FinishTime      { get; set; } = string.Empty;
        public int?     Position        { get; set; }
        public string?  Notes           { get; set; }
    }

    public class CreateResultViewModel
    {
        public int     EnrolmentID { get; set; }
        public string  FinishTime  { get; set; } = string.Empty;
        public int?    Position    { get; set; }
        public string? Notes       { get; set; }
    }

    // Sent to API PUT /api/results/{id}
    public class UpdateResultApiDto
    {
        public string? FinishTime { get; set; }
        public int?    Position   { get; set; }
        public string? Notes      { get; set; }
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public class OrganiserDashboardViewModel
    {
        public List<EventDto> Events          { get; set; } = new();
        public int            TotalEvents     { get; set; }
        public int            TotalEnrolments { get; set; }
        public List<EventDto> UpcomingEvents  { get; set; } = new();
    }
}
