using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Routes")]
    public class Route
    {
        [Key]
        public int RouteID { get; set; }

        [Required]
        public int EventID { get; set; }

        [MaxLength(500)]
        public string? RouteMapURL { get; set; }

        [Required, MaxLength(200)]
        public string StartPoint { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string EndPoint { get; set; } = string.Empty;

        public decimal? ElevationGain { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Navigation
        [ForeignKey("EventID")]
        public Event? Event { get; set; }
    }
}
