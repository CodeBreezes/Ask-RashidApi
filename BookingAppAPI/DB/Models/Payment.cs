using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAppAPI.DB.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public string? StripePaymentIntentId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public long Amount { get; set; }

        public string Currency { get; set; } = "AED";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? BookingId { get; set; }

        public int? userId { get; set; }
    }
}
