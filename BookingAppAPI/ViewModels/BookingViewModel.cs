using System.ComponentModel.DataAnnotations;

namespace BookingAppAPI.ViewModels
{
    public class BookingViewModel
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateOnly StartedDate { get; set; }

        [Required]
        public TimeOnly StartedTime { get; set; }

        [Required]
        public string? Topic { get; set; } = string.Empty;
        [Required]
        public string? Notes { get; set; } = string.Empty;
    }
}
