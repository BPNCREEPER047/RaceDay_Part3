using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Enrolments")]
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int EventID { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Registered";  // "Registered" or "Cancelled"

        // Navigation properties
        [ForeignKey("ParticipantID")]
        public User? Participant { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        public Result? Result { get; set; }
    }
}
