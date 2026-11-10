using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    // ── Category ──────────────────────────────────────────────────────────────
    public class CreateCategoryDto
    {
        [Required] public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CategoryResponseDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // ── Event ─────────────────────────────────────────────────────────────────
    public class CreateEventDto
    {
        [Required] public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required] public DateTime EventDate { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        [Required, Range(0.1, 9999)] public decimal Distance { get; set; }
        public int MaxParticipants { get; set; } = 100;
        [Required] public int CategoryID { get; set; }
        // "run", "walk", "cycle"
        public string EventType { get; set; } = "run";
        public string? BannerImageUrl { get; set; }
    }

    public class EventResponseDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int MaxParticipants { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int OrganiserID { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        // NEW fields required by MVC
        public string EventType { get; set; } = "run";
        public string? BannerImageUrl { get; set; }
    }

    // ── Enrolment ─────────────────────────────────────────────────────────────
    public class CreateEnrolmentDto
    {
        [Required] public int EventID { get; set; }
    }

    public class EnrolmentResponseDto
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        // NEW field required by MVC
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ── Result ────────────────────────────────────────────────────────────────
    public class CreateResultDto
    {
        [Required] public int EnrolmentID { get; set; }
        [Required] public string FinishTime { get; set; } = string.Empty;
        public int? Position { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateResultDto
    {
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public string? Notes { get; set; }
    }

    public class ResultResponseDto
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int? Position { get; set; }
        public string? Notes { get; set; }
    }

    // ── Route ─────────────────────────────────────────────────────────────────
    public class CreateRouteDto
    {
        [Required] public int EventID { get; set; }
        public string? RouteMapURL { get; set; }
        [Required] public string StartPoint { get; set; } = string.Empty;
        [Required] public string EndPoint { get; set; } = string.Empty;
        public decimal? ElevationGain { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateRouteDto
    {
        public string? RouteMapURL { get; set; }
        public string? StartPoint { get; set; }
        public string? EndPoint { get; set; }
        public decimal? ElevationGain { get; set; }
        public string? Description { get; set; }
    }

    public class RouteResponseDto
    {
        public int RouteID { get; set; }
        public int EventID { get; set; }
        public string? RouteMapURL { get; set; }
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public decimal? ElevationGain { get; set; }
        public string? Description { get; set; }
    }
}
