using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAppAPI.DB.Models
{
    public class PaymentRequest
    {
        [Key]
        public int UniqueId { get; set; } // <-- This is the primary key
        public string PhoneNumber { get; set; } = string.Empty;

        public long Amount { get; set; }

        public string Currency { get; set; } = "AED";  

        public string? Description { get; set; } 

        public string? CustomerName { get; set; } 

        public string? Email { get; set; }

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;


        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
