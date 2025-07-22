namespace BookingAppAPI.DB.Models
{
    public class PaymentRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;

        public long Amount { get; set; }

        public string Currency { get; set; } = "AED";  

        public string? Description { get; set; } 

        public string? CustomerName { get; set; } 

        public string? Email { get; set; }  

        public string? BookingId { get; set; } 
    }
}
