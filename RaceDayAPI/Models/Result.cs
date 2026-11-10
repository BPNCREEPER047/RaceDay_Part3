using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required, MaxLength(20)]
        public string FinishTime { get; set; } = string.Empty;

        public int? Position { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey("EnrolmentID")]
        public Enrolment? Enrolment { get; set; }
    }
}
