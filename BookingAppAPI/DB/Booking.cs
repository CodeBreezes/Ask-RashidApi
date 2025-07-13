using BookingAppAPI.DB.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAppAPI.DB
{
    public class Booking
    {
        [Key]
        public int UniqueId { get; set; }
 
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Services Service { get; set; } = null!;

        public int UserId { get; set; }
        [ForeignKey("UserId")]

        public AppUser User { get; set; } = null!;
        public DateOnly StartedDate { get; set; }
        public TimeOnly StartedTime { get; set; }
        public DateTime? EndedDate { get; set; }
        public  string? Topic { get; set; } = string.Empty;
        public string? Notes { get; set; } = string.Empty;
    }
}
