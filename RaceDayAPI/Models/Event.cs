using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Events")]
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required, MaxLength(200)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        public int MaxParticipants { get; set; } = 100;

        [Required, MaxLength(20)]
        public string EventType { get; set; } = "run";   // "run" | "walk" | "cycle"

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        [ForeignKey("OrganiserID")]
        public User? Organiser { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
        public Route? Route { get; set; }
    }
}
