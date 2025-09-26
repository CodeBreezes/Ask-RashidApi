namespace BookingAppAPI.ViewModels
{
  
        public class BookingDetailsViewModel
        {
            // Booking info
            public int UniqueId { get; set; }
            public string ServiceName { get; set; } = string.Empty;
            public DateOnly StartedDate { get; set; }
            public TimeOnly StartedTime { get; set; }
            public DateTime? EndedDate { get; set; }
            public string? Topic { get; set; }
            public string? Notes { get; set; }

            // User info (all fields)
            public int UserUniqueId { get; set; }
            
            public string FullName { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string? Gender { get; set; }
           
            public string? ProfileImageUrl { get; set; }
            public string? Address { get; set; }
          
        }
    }
