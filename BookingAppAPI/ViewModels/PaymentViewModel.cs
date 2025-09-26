namespace BookingAppAPI.ViewModels
{
    public class PaymentViewModel
    {

        public int Id { get; set; } // <-- Needed for Index/Details
        public string StripePaymentIntentId { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string PhoneNumber { get; set; } = string.Empty;
            public long Amount { get; set; }
            public string Currency { get; set; } = "AED"; // force AED
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? BookingId { get; set; }
            public int? ServiceId { get; set; }

            // Booking info
            public string? ServiceName { get; set; }

            // User info
            public int? UserId { get; set; }
            public int? UserUniqueId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string? UserPhoneNumber { get; set; }
            public string? UserEmail { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? Gender { get; set; }
            public string? ProfileImageUrl { get; set; }
            public string? Address { get; set; }
        }
    }


