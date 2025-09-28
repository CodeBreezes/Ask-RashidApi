namespace BookingAppAPI.ViewModels
{
    public class FeedbackViewModel
    {

            // Feedback Info
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Category { get; set; }
            public string? Message { get; set; }
            public DateTime CreatedDate { get; set; }

            // User Info (optional)
            public int? UserId { get; set; }
            public int? UserUniqueId { get; set; }
            public string? FullName { get; set; }
            public string? UserEmail { get; set; }
            public string? UserPhoneNumber { get; set; }
            public string? ProfileImageUrl { get; set; }
            public string? Address { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? Gender { get; set; }
        }
    }